using System.Security.Cryptography;
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
        private readonly IDataProtector _protector;

        public GarminServicio(
            IGarminConnectCliente garmin,
            ICuentaGarminRepositorio cuentas,
            IEntrenamientoRepositorio entrenamientos,
            IDataProtectionProvider proteccion)
        {
            _garmin = garmin;
            _cuentas = cuentas;
            _entrenamientos = entrenamientos;
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
            var sinActividad = new List<string>();
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

                if (coincidencias.Count == 1)
                {
                    pares.Add((vinculo, coincidencias[0]));
                }
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

                var elegida = Elegir(actividades, entrenamiento.Turno);
                if (elegida is null)
                {
                    sinActividad.Add($"{vinculo.Atleta.Apellido}, {vinculo.Atleta.Nombre}");
                    continue;
                }

                elegida = await EjecutarAsync(
                    cuenta,
                    access => _garmin.CompletarAsync(access, elegida, cancelacion),
                    cancelacion);

                Copiar(vinculo, elegida);
                asignadas++;
            }

            await _entrenamientos.ActualizarAsync(entrenamiento);

            var huboCruce = asignadas > 0 || sinActividad.Count > 0;
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
            else if (asignadas == 0)
            {
                avisos.Insert(0, "El nombre coincide, pero ese día no hay una actividad en Garmin.");
            }
            else
            {
                avisos.Insert(0, asignadas == 1
                    ? "Se encontró 1 actividad."
                    : $"Se encontraron {asignadas} actividades.");

                foreach (var nombre in entrenamiento.Atletas
                    .Where(v => v.GarminActividadId is null)
                    .Select(v => $"{v.Atleta.Apellido}, {v.Atleta.Nombre}")
                    .Except(sinActividad))
                {
                    avisos.Add($"{nombre} no coincide con ningún amigo de Garmin.");
                }
            }

            foreach (var nombre in sinActividad)
            {
                avisos.Add($"{nombre} coincide con un amigo, pero no tiene una actividad ese día.");
            }

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
            var sesion = await _garmin.RefrescarAsync(
                Desproteger(cuenta.RefreshToken),
                cuenta.ClientId,
                cancelacion);

            cuenta.AccessToken = _protector.Protect(sesion.AccessToken);
            cuenta.RefreshToken = _protector.Protect(sesion.RefreshToken);
            cuenta.ClientId = sesion.ClientId;
            cuenta.ExpiraUtc = sesion.ExpiraUtc;
            await _cuentas.GuardarAsync(cuenta);
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
        /// Si hay varias actividades ese dia, prioriza la del turno
        /// (mañana antes de las 14, tarde desde las 14) y, dentro de eso, la más larga.
        /// </summary>
        private static ActividadGarminLeida? Elegir(IReadOnlyList<ActividadGarminLeida> actividades, Turno turno)
        {
            if (actividades.Count == 0)
            {
                return null;
            }

            var delTurno = actividades.Where(a => EntraEnElTurno(a, turno)).ToList();
            var candidatas = delTurno.Count > 0 ? delTurno : actividades;

            return candidatas
                .OrderByDescending(a => a.DuracionSegundos ?? 0)
                .First();
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
