using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using AccesoDatos.Repositorios;
using Entidades;
using Entidades.DTOs;
using Entidades.Enums;
using Entidades.Mapeos;
using Microsoft.AspNetCore.DataProtection;

namespace Controladores.Integraciones
{
    public interface IGarminServicio
    {
        Task<EstadoGarminDto> EstadoAsync(int usuarioId);

        Task<EstadoGarminDto> VincularAsync(int usuarioId, string usuario, string password, CancellationToken cancelacion);

        Task DesvincularAsync(int usuarioId);

        Task<SincronizacionGarminDto?> SincronizarAsync(int usuarioId, int entrenamientoId, CancellationToken cancelacion);

        Task<CalendarioGarminDto> CalendarioAsync(int usuarioId, DateOnly desde, bool mes, CancellationToken cancelacion);
    }

    /// <summary>
    /// Vincula la cuenta del entrenador y cruza las actividades del dia
    /// de sus amigos de Garmin con los atletas de la sesion, por nombre.
    /// </summary>
    public class GarminServicio : IGarminServicio
    {
        private readonly IGarminConnectCliente _garmin;
        private readonly ICuentaGarminRepositorio _cuentas;
        private readonly IEntrenamientoRepositorio _entrenamientos;
        private readonly ITrabajoRepositorio _trabajos;
        private readonly IDataProtector _protector;
        private readonly SemaphoreSlim _renovacion = new(1, 1);
        private int _renovaciones;

        public GarminServicio(
            IGarminConnectCliente garmin,
            ICuentaGarminRepositorio cuentas,
            IEntrenamientoRepositorio entrenamientos,
            ITrabajoRepositorio trabajos,
            IDataProtectionProvider proteccion)
        {
            _garmin = garmin;
            _cuentas = cuentas;
            _entrenamientos = entrenamientos;
            _trabajos = trabajos;
            _protector = proteccion.CreateProtector("CoachApp.Garmin.Tokens.v1");
        }

        public async Task<EstadoGarminDto> EstadoAsync(int usuarioId)
        {
            var cuenta = await _cuentas.ObtenerPorUsuarioAsync(usuarioId);
            return cuenta is null
                ? new EstadoGarminDto { Vinculada = false }
                : new EstadoGarminDto
                {
                    Vinculada = true,
                    Nombre = cuenta.NombreCompleto ?? cuenta.DisplayName
                };
        }

        public async Task<EstadoGarminDto> VincularAsync(
            int usuarioId,
            string usuario,
            string password,
            CancellationToken cancelacion)
        {
            var sesion = await _garmin.IniciarSesionAsync(usuario.Trim(), password, cancelacion);
            var perfil = await _garmin.ObtenerPerfilAsync(sesion.AccessToken, cancelacion);

            var cuenta = await _cuentas.ObtenerPorUsuarioAsync(usuarioId) ?? new CuentaGarmin
            {
                UsuarioId = usuarioId
            };

            cuenta.DisplayName = perfil.DisplayName;
            cuenta.NombreCompleto = perfil.NombreCompleto;
            cuenta.AccessToken = _protector.Protect(sesion.AccessToken);
            cuenta.RefreshToken = _protector.Protect(sesion.RefreshToken);
            cuenta.ClientId = sesion.ClientId;
            cuenta.ExpiraUtc = sesion.ExpiraUtc;

            await _cuentas.GuardarAsync(cuenta);

            return new EstadoGarminDto
            {
                Vinculada = true,
                Nombre = cuenta.NombreCompleto ?? cuenta.DisplayName
            };
        }

        public async Task DesvincularAsync(int usuarioId)
        {
            var cuenta = await _cuentas.ObtenerPorUsuarioAsync(usuarioId);
            if (cuenta is not null)
            {
                await _cuentas.EliminarAsync(cuenta);
            }
        }

        public async Task<CalendarioGarminDto> CalendarioAsync(
            int usuarioId,
            DateOnly desde,
            bool mes,
            CancellationToken cancelacion)
        {
            var cuenta = await _cuentas.ObtenerPorUsuarioAsync(usuarioId)
                ?? throw new GarminExcepcion("Todavía no vinculaste una cuenta de Garmin.");

            var inicio = mes ? new DateOnly(desde.Year, desde.Month, 1) : LunesDe(desde);
            var fin = mes ? inicio.AddMonths(1).AddDays(-1) : inicio.AddDays(6);
            GarminExcepcion? bloqueo = null;
            List<AmigoGarmin> amigos;
            try
            {
                amigos = (await EjecutarAsync(
                        cuenta,
                        access => _garmin.ObtenerAmigosAsync(access, cancelacion),
                        cancelacion))
                    .ToList();
            }
            catch (GarminExcepcion ex) when (ex.Prohibido)
            {
                bloqueo = ex;
                amigos = [];
            }

            IReadOnlyList<ActividadAmigoGarmin> feed;
            var feedProhibido = false;
            try
            {
                feed = await EjecutarAsync(
                    cuenta,
                    access => _garmin.ObtenerFeedEntreAsync(access, inicio, fin, cancelacion),
                    cancelacion);
            }
            catch (GarminExcepcion ex) when (ex.Prohibido)
            {
                bloqueo ??= ex;
                feed = [];
                feedProhibido = true;
            }

            if (amigos.Count == 0 && feed.Count == 0 && bloqueo is not null)
            {
                throw bloqueo;
            }

            var conocidos = new HashSet<string>(amigos.Select(a => a.DisplayName), StringComparer.OrdinalIgnoreCase);
            foreach (var novedad in feed)
            {
                if (conocidos.Add(novedad.DisplayName))
                {
                    amigos.Add(new AmigoGarmin(novedad.DisplayName, novedad.Nombre));
                }
            }

            amigos = amigos
                .OrderBy(a => a.NombreCompleto, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var cargados = new Dictionary<string, HashSet<DateOnly>>(StringComparer.OrdinalIgnoreCase);
            var actividadesProhibidas = 0;
            using var puerta = new SemaphoreSlim(4);
            var tareas = amigos.Select(async amigo =>
            {
                await puerta.WaitAsync(cancelacion);
                try
                {
                    IReadOnlyList<ActividadGarminLeida> actividades;
                    try
                    {
                        actividades = await EjecutarAsync(
                            cuenta,
                            access => _garmin.ObtenerActividadesEntreAsync(
                                access,
                                amigo.DisplayName,
                                inicio,
                                fin,
                                cancelacion),
                            cancelacion);
                    }
                    catch (GarminExcepcion ex) when (ex.Prohibido)
                    {
                        Interlocked.Increment(ref actividadesProhibidas);
                        lock (cargados)
                        {
                            bloqueo ??= ex;
                        }

                        actividades = [];
                    }

                    var dias = actividades
                        .Where(a => a.InicioLocal is not null)
                        .Select(a => DateOnly.FromDateTime(a.InicioLocal!.Value))
                        .ToHashSet();

                    lock (cargados)
                    {
                        cargados[amigo.DisplayName] = dias;
                    }
                }
                finally
                {
                    puerta.Release();
                }
            });

            await Task.WhenAll(tareas);

            if (feedProhibido && amigos.Count > 0 && actividadesProhibidas == amigos.Count && bloqueo is not null)
            {
                throw bloqueo;
            }

            foreach (var novedad in feed)
            {
                if (!cargados.TryGetValue(novedad.DisplayName, out var dias))
                {
                    dias = [];
                    cargados[novedad.DisplayName] = dias;
                }

                dias.Add(novedad.Fecha);
            }

            var calendario = new CalendarioGarminDto
            {
                Desde = inicio,
                Hasta = fin
            };

            for (var dia = inicio; dia <= fin; dia = dia.AddDays(1))
            {
                calendario.Dias.Add(new DiaCalendarioGarminDto
                {
                    Fecha = dia,
                    Amigos = amigos.Select(amigo => new AmigoCalendarioGarminDto
                    {
                        Nombre = string.IsNullOrWhiteSpace(amigo.NombreCompleto)
                            ? amigo.DisplayName
                            : amigo.NombreCompleto,
                        Clave = amigo.DisplayName,
                        Cargo = cargados.TryGetValue(amigo.DisplayName, out var dias) && dias.Contains(dia)
                    }).ToList()
                });
            }

            return calendario;
        }

        /// <summary>Lunes de la semana que contiene la fecha. La semana empieza el lunes.</summary>
        private static DateOnly LunesDe(DateOnly fecha)
        {
            var corrimiento = ((int)fecha.DayOfWeek + 6) % 7;
            return fecha.AddDays(-corrimiento);
        }

        public async Task<SincronizacionGarminDto?> SincronizarAsync(
            int usuarioId,
            int entrenamientoId,
            CancellationToken cancelacion)
        {
            var entrenamiento = await _entrenamientos.ObtenerPorIdAsync(entrenamientoId);
            if (entrenamiento is null)
            {
                return null;
            }

            var cuenta = await _cuentas.ObtenerPorUsuarioAsync(usuarioId)
                ?? throw new GarminExcepcion("Todavía no vinculaste una cuenta de Garmin.");

            var token = await TokenVigenteAsync(cuenta, cancelacion);
            var amigos = await EjecutarAsync(
                cuenta,
                access => _garmin.ObtenerAmigosAsync(access, cancelacion),
                cancelacion,
                token);

            var avisos = new List<string>();

            foreach (var vinculo in entrenamiento.Atletas)
            {
                Limpiar(vinculo);
            }

            var asignadas = 0;
            var trabajosNuevos = 0;
            var sinCoincidencia = new List<string>();
            var sinActividad = new List<string>();
            var sinDeporte = new List<string>();
            var pares = new List<(EntrenamientoAtleta Vinculo, AmigoGarmin Amigo)>();

            foreach (var vinculo in entrenamiento.Atletas)
            {
                var coincidencias = amigos
                    .Where(a => NombresGarmin.Coincide(vinculo.Atleta.Nombre, vinculo.Atleta.Apellido, a.NombreCompleto))
                    .ToList();

                if (coincidencias.Count > 1)
                {
                    avisos.Add($"{vinculo.Atleta.Apellido}, {vinculo.Atleta.Nombre} coincide con más de un amigo de Garmin.");
                    continue;
                }

                if (coincidencias.Count == 0)
                {
                    sinCoincidencia.Add($"{vinculo.Atleta.Apellido}, {vinculo.Atleta.Nombre}");
                    continue;
                }

                pares.Add((vinculo, coincidencias[0]));
            }

            foreach (var grupo in pares.GroupBy(p => p.Amigo.DisplayName, StringComparer.OrdinalIgnoreCase))
            {
                if (grupo.Count() > 1)
                {
                    avisos.Add($"Hay más de un atleta que coincide con {grupo.First().Amigo.NombreCompleto}.");
                    continue;
                }

                var (vinculo, amigo) = grupo.First();
                var actividades = await EjecutarAsync(
                    cuenta,
                    access => _garmin.ObtenerActividadesDelDiaAsync(access, amigo.DisplayName, entrenamiento.Fecha, cancelacion),
                    cancelacion);

                var elegida = Elegir(actividades, entrenamiento.Turno, entrenamiento.Descripcion);
                if (elegida is null)
                {
                    await QuitarTrabajoDeGarminAsync(vinculo);
                    var nombre = $"{vinculo.Atleta.Apellido}, {vinculo.Atleta.Nombre}";
                    if (actividades.Count > 0 && TiposPedidos(entrenamiento.Descripcion).Count > 0)
                    {
                        sinDeporte.Add(nombre);
                    }
                    else
                    {
                        sinActividad.Add(nombre);
                    }

                    continue;
                }

                elegida = await EjecutarAsync(
                    cuenta,
                    access => _garmin.CompletarAsync(access, elegida, cancelacion),
                    cancelacion);

                Copiar(vinculo, elegida);
                if (await GuardarTrabajoAsync(vinculo, elegida))
                {
                    trabajosNuevos++;
                }

                asignadas++;
            }

            await _entrenamientos.ActualizarAsync(entrenamiento);

            var huboCruce = asignadas > 0 || sinActividad.Count > 0 || sinDeporte.Count > 0;
            if (!huboCruce)
            {
                avisos.Insert(0, "Ningún atleta de la sesión coincide con un amigo de Garmin.");
                var nombres = amigos
                    .Select(a => a.NombreCompleto)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Order(StringComparer.OrdinalIgnoreCase)
                    .Take(12)
                    .ToList();

                if (nombres.Count > 0)
                {
                    avisos.Add("En Garmin tus amigos figuran como: " + string.Join(", ", nombres) + ".");
                }
                else
                {
                    avisos.Add("La cuenta de Garmin no tiene amigos para leer.");
                }

                avisos.Add("Alcanza con que el nombre del atleta sea el del amigo en Garmin. El apellido no hace falta si Garmin no lo muestra.");
            }
            else
            {
                if (asignadas > 0)
                {
                    avisos.Insert(0, asignadas == 1
                        ? "Se encontró 1 actividad."
                        : $"Se encontraron {asignadas} actividades.");
                }

                if (trabajosNuevos > 0)
                {
                    avisos.Insert(asignadas > 0 ? 1 : 0, trabajosNuevos == 1
                        ? "Se sumó 1 trabajo al historial."
                        : $"Se sumaron {trabajosNuevos} trabajos al historial.");
                }

                AgregarResumen(
                    avisos,
                    sinCoincidencia,
                    "no coincide con un amigo de Garmin",
                    "no coinciden con un amigo de Garmin");
            }

            AgregarResumen(
                avisos,
                sinActividad,
                "coincide con un amigo, pero no tiene actividad ese día",
                "coinciden con un amigo, pero no tienen actividad ese día");

            AgregarResumen(
                avisos,
                sinDeporte,
                "tiene otras actividades en Garmin, pero ninguna es la de la descripción. Quedó vacío para cargarlo a mano",
                "tienen otras actividades en Garmin, pero ninguna es la de la descripción. Quedaron vacíos para cargarlos a mano");

            return new SincronizacionGarminDto
            {
                Entrenamiento = entrenamiento.ToDto(),
                Avisos = avisos
            };
        }

        private async Task<string> TokenVigenteAsync(CuentaGarmin cuenta, CancellationToken cancelacion)
        {
            if (cuenta.ExpiraUtc <= DateTime.UtcNow.AddMinutes(2))
            {
                await RenovarAsync(cuenta, cancelacion);
            }

            return Desproteger(cuenta.AccessToken);
        }

        private async Task<T> EjecutarAsync<T>(
            CuentaGarmin cuenta,
            Func<string, Task<T>> accion,
            CancellationToken cancelacion,
            string? token = null)
        {
            token ??= await TokenVigenteAsync(cuenta, cancelacion);

            try
            {
                return await accion(token);
            }
            catch (GarminExcepcion ex) when (ex.SesionVencida)
            {
                await RenovarAsync(cuenta, cancelacion);
                return await accion(Desproteger(cuenta.AccessToken));
            }
        }

        private async Task RenovarAsync(CuentaGarmin cuenta, CancellationToken cancelacion)
        {
            // El calendario consulta varios amigos a la vez. Si el token vencio,
            // una sola renovacion tiene que servirles a todas: Garmin invalida
            // el refresh anterior en cuanto se usa.
            var vista = Volatile.Read(ref _renovaciones);
            await _renovacion.WaitAsync(cancelacion);
            try
            {
                if (Volatile.Read(ref _renovaciones) != vista)
                {
                    return;
                }

                var sesion = await _garmin.RefrescarAsync(
                    Desproteger(cuenta.RefreshToken),
                    cuenta.ClientId,
                    cancelacion);

                cuenta.AccessToken = _protector.Protect(sesion.AccessToken);
                cuenta.RefreshToken = _protector.Protect(sesion.RefreshToken);
                cuenta.ClientId = sesion.ClientId;
                cuenta.ExpiraUtc = sesion.ExpiraUtc;
                await _cuentas.GuardarAsync(cuenta);
                Interlocked.Increment(ref _renovaciones);
            }
            finally
            {
                _renovacion.Release();
            }
        }

        private string Desproteger(string valor)
        {
            try
            {
                return _protector.Unprotect(valor);
            }
            catch (CryptographicException)
            {
                throw new GarminExcepcion("La sesión de Garmin guardada ya no sirve. Vinculá la cuenta de nuevo.");
            }
        }

        /// <summary>
        /// Si la descripcion de la sesion nombra un deporte, se queda con esas
        /// actividades. Si no, usa agua y ciclismo. Despues prioriza el turno
        /// (mañana antes de las 14, tarde desde las 14) y, dentro de eso, la más larga.
        /// </summary>
        private static ActividadGarminLeida? Elegir(
            IReadOnlyList<ActividadGarminLeida> actividades,
            Turno turno,
            string? descripcion)
        {
            if (actividades.Count == 0)
            {
                return null;
            }

            var pedidos = TiposPedidos(descripcion);
            var candidatas = pedidos.Count == 0
                ? actividades.Where(EsAguaOCiclismo).ToList()
                : actividades.Where(a => a.Tipo is not null && pedidos.Contains(a.Tipo)).ToList();

            if (candidatas.Count == 0)
            {
                return null;
            }

            var delTurno = candidatas.Where(a => EntraEnElTurno(a, turno)).ToList();
            var delDia = delTurno.Count > 0 ? delTurno : candidatas;

            return delDia
                .OrderByDescending(a => a.DuracionSegundos ?? 0)
                .First();
        }

        /// <summary>Agua y ciclismo son lo que se lee cuando la sesion no nombra otro deporte.</summary>
        private static bool EsAguaOCiclismo(ActividadGarminLeida actividad) =>
            actividad.Tipo is "Remo" or "Kayak" or "Natación" or "Ciclismo";

        /// <summary>
        /// Deportes nombrados en la descripcion. "Carrera continua 20'" pide
        /// las carreras y deja afuera el ciclismo del mismo dia.
        /// </summary>
        private static HashSet<string> TiposPedidos(string? descripcion)
        {
            if (string.IsNullOrWhiteSpace(descripcion))
            {
                return [];
            }

            var texto = Normalizar(descripcion);
            if (ContienePalabra(texto, "carrera") || ContienePalabra(texto, "correr"))
            {
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Carrera" };
            }

            if (ContienePalabra(texto, "caminata") || ContienePalabra(texto, "caminar"))
            {
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Caminata" };
            }

            if (ContienePalabra(texto, "ciclismo") || ContienePalabra(texto, "bici") || ContienePalabra(texto, "bicicleta"))
            {
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Ciclismo" };
            }

            if (ContienePalabra(texto, "remo"))
            {
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Remo" };
            }

            if (ContienePalabra(texto, "kayak"))
            {
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Kayak" };
            }

            if (ContienePalabra(texto, "agua") || ContienePalabra(texto, "natacion"))
            {
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Remo", "Kayak", "Natación" };
            }

            if (ContienePalabra(texto, "fuerza") || ContienePalabra(texto, "gimnasio"))
            {
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Fuerza" };
            }

            return [];
        }

        private static bool ContienePalabra(string texto, string palabra) =>
            Regex.IsMatch(texto, $@"\b{Regex.Escape(palabra)}\b", RegexOptions.CultureInvariant);

        private static string Normalizar(string texto)
        {
            var descompuesto = texto.Normalize(NormalizationForm.FormD);
            var plano = new StringBuilder(descompuesto.Length);

            foreach (var caracter in descompuesto)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(caracter) != UnicodeCategory.NonSpacingMark)
                {
                    plano.Append(char.ToLowerInvariant(caracter));
                }
            }

            return plano.ToString().Replace('ñ', 'n');
        }

        private static bool EntraEnElTurno(ActividadGarminLeida actividad, Turno turno)
        {
            if (actividad.InicioLocal is null)
            {
                return false;
            }

            var hora = actividad.InicioLocal.Value.Hour;
            return turno == Turno.Manana ? hora < 14 : hora >= 14;
        }

        /// <summary>
        /// Saca el trabajo que se habia armado desde Garmin. Los cargados a mano
        /// quedan, para completar la sesion cuando el deporte pedido no esta.
        /// </summary>
        private async Task QuitarTrabajoDeGarminAsync(EntrenamientoAtleta vinculo)
        {
            var importados = vinculo.Trabajos.Where(t => t.GarminActividadId is not null).ToList();
            foreach (var trabajo in importados)
            {
                vinculo.Trabajos.Remove(trabajo);
                await _trabajos.EliminarAsync(trabajo);
            }
        }

        /// <summary>
        /// Crea o actualiza el trabajo que sale en el historial. El de Garmin
        /// se reemplaza al volver a traer; los cargados a mano no se tocan.
        /// </summary>
        private async Task<bool> GuardarTrabajoAsync(EntrenamientoAtleta vinculo, ActividadGarminLeida actividad)
        {
            var marcas = Marcas(actividad);
            if (marcas.Count == 0 || TipoDe(actividad.Tipo) == TipoTrabajo.Gimnasio)
            {
                return false;
            }

            var tipo = TipoDe(actividad.Tipo);
            var hora = actividad.InicioLocal is null ? (TimeOnly?)null : TimeOnly.FromDateTime(actividad.InicioLocal.Value);
            var parciales = marcas.Select(m => new TrabajoParcial
            {
                DistanciaMetros = m.Metros,
                Tiempo = m.Tiempo
            });
            var paladas = tipo == TipoTrabajo.Agua
                ? marcas.Where(m => m.Ppm is not null).Select(m => new TrabajoPalada
                {
                    Tiempo = m.MomentoPpm,
                    Ppm = m.Ppm!.Value
                })
                : [];

            var existente = vinculo.Trabajos.FirstOrDefault(t => t.GarminActividadId is not null);
            if (existente is null)
            {
                var trabajo = new Trabajo
                {
                    EntrenamientoId = vinculo.EntrenamientoId,
                    AtletaId = vinculo.AtletaId,
                    Tipo = tipo,
                    HoraInicio = hora,
                    Observaciones = Nota(actividad),
                    GarminActividadId = actividad.ActividadId,
                    FechaCreacion = DateTime.UtcNow
                };

                await _trabajos.CrearAsync(trabajo, parciales, [], paladas);
                if (!vinculo.Trabajos.Contains(trabajo))
                {
                    vinculo.Trabajos.Add(trabajo);
                }

                return true;
            }

            existente.Tipo = tipo;
            existente.HoraInicio = hora;
            existente.Observaciones = Nota(actividad);
            existente.GarminActividadId = actividad.ActividadId;
            await _trabajos.ActualizarAsync(existente, parciales, [], paladas);
            return true;
        }

        /// <summary>En Garmin el ciclismo es el registro del agua.</summary>
        private static TipoTrabajo TipoDe(string? tipoGarmin) => tipoGarmin switch
        {
            "Carrera" or "Caminata" => TipoTrabajo.Tierra,
            "Ciclismo" => TipoTrabajo.Agua,
            "Fuerza" => TipoTrabajo.Gimnasio,
            _ => TipoTrabajo.Agua
        };

        private static string? Nota(ActividadGarminLeida actividad)
        {
            var partes = new List<string>();
            if (!string.IsNullOrWhiteSpace(actividad.Nombre) &&
                !actividad.Nombre.Equals("Actividad", StringComparison.OrdinalIgnoreCase))
            {
                partes.Add(actividad.Nombre.Trim());
            }

            if (actividad.FcPromedio is int fc)
            {
                partes.Add(actividad.FcMaxima is int max ? $"FC {fc} (máx {max})" : $"FC {fc}");
            }

            if (partes.Count == 0)
            {
                return "Garmin";
            }

            var nota = "Garmin. " + string.Join(". ", partes);
            return nota.Length <= 300 ? nota : nota[..300];
        }

        private static List<(int Metros, TimeSpan Tiempo, TimeSpan MomentoPpm, int? Ppm)> Marcas(ActividadGarminLeida actividad)
        {
            var tramos = (actividad.Tramos ?? [])
                .Where(t => t.DistanciaMetros >= 1 || t.DuracionSegundos >= 1)
                .ToList();

            if (tramos.Count == 0)
            {
                if ((actividad.DistanciaMetros ?? 0) < 1 && (actividad.DuracionSegundos ?? 0) < 1)
                {
                    return [];
                }

                var totalSegundos = actividad.DuracionSegundos ?? 0;
                return [Marca(actividad.DistanciaMetros ?? 0, totalSegundos, totalSegundos / 2, actividad.Cadencia)];
            }

            var suma = tramos.Sum(t => t.DistanciaMetros);
            var total = actividad.DistanciaMetros ?? suma;
            var acumulados = tramos.Count > 1
                && tramos[^1].DistanciaMetros > tramos[0].DistanciaMetros
                && Math.Abs(tramos[^1].DistanciaMetros - total) <= Math.Abs(suma - total);

            double distancia = 0;
            double segundos = 0;
            var marcas = new List<(int Metros, TimeSpan Tiempo, TimeSpan MomentoPpm, int? Ppm)>(tramos.Count);

            foreach (var tramo in tramos)
            {
                var inicio = segundos;
                if (acumulados)
                {
                    distancia = tramo.DistanciaMetros;
                    segundos = tramo.DuracionSegundos;
                }
                else
                {
                    distancia += tramo.DistanciaMetros;
                    segundos += tramo.DuracionSegundos;
                }

                // Se guarda cuanto duro el tramo, no la marca acumulada: las paladas
                // se ubican sumando esas duraciones sobre el cronometro corrido.
                // La muestra va a la mitad del tramo para que caiga en ese parcial
                // y no en el siguiente, que arranca cuando el tiempo es igual.
                var duracion = Math.Max(segundos - inicio, 0);
                var medio = inicio + duracion / 2;
                marcas.Add(Marca(distancia, duracion, medio, tramo.Cadencia ?? actividad.Cadencia));
            }

            return marcas;
        }

        private static (int Metros, TimeSpan Tiempo, TimeSpan MomentoPpm, int? Ppm) Marca(
            double metros,
            double segundos,
            double momentoPpm,
            double? cadencia)
        {
            int? ppm = null;
            if (cadencia is >= 1 and <= 120)
            {
                ppm = (int)Math.Round(cadencia.Value);
            }

            return (
                (int)Math.Round(metros),
                TimeSpan.FromSeconds(Math.Max(segundos, 0)),
                TimeSpan.FromSeconds(Math.Max(momentoPpm, 0)),
                ppm);
        }

        private static void Limpiar(EntrenamientoAtleta vinculo)
        {
            vinculo.GarminActividadId = null;
            vinculo.GarminNombre = null;
            vinculo.GarminTipo = null;
            vinculo.GarminInicio = null;
            vinculo.GarminDistanciaMetros = null;
            vinculo.GarminDuracionSegundos = null;
            vinculo.GarminFcPromedio = null;
            vinculo.GarminFcMaxima = null;
            vinculo.GarminCadencia = null;
            vinculo.GarminCalorias = null;
        }

        private static void Copiar(EntrenamientoAtleta vinculo, ActividadGarminLeida actividad)
        {
            vinculo.GarminActividadId = actividad.ActividadId;
            vinculo.GarminNombre = Recortar(actividad.Nombre, 200);
            vinculo.GarminTipo = Recortar(actividad.Tipo, 80);
            vinculo.GarminInicio = actividad.InicioLocal;
            vinculo.GarminDistanciaMetros = actividad.DistanciaMetros;
            vinculo.GarminDuracionSegundos = actividad.DuracionSegundos;
            vinculo.GarminFcPromedio = actividad.FcPromedio;
            vinculo.GarminFcMaxima = actividad.FcMaxima;
            vinculo.GarminCadencia = actividad.Cadencia;
            vinculo.GarminCalorias = actividad.Calorias;
        }

        private static void AgregarResumen(
            List<string> avisos,
            List<string> nombres,
            string uno,
            string varios)
        {
            if (nombres.Count == 0)
            {
                return;
            }

            avisos.Add(nombres.Count == 1
                ? $"{nombres[0]} {uno}."
                : $"{nombres.Count} atletas {varios}: {string.Join("; ", nombres)}.");
        }

        private static string? Recortar(string? texto, int maximo)
        {
            if (string.IsNullOrWhiteSpace(texto))
            {
                return null;
            }

            var limpio = texto.Trim();
            return limpio.Length <= maximo ? limpio : limpio[..maximo];
        }
    }
}
