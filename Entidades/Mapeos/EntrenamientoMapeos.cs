using Entidades.DTOs;
using Entidades.Enums;
using Entidades.Util;

namespace Entidades.Mapeos
{
    /// <summary>Conversion de la entidad Entrenamiento a su DTO publico.</summary>
    public static class EntrenamientoMapeos
    {
        /// <summary>
        /// Arma el DTO completo de la sesion, con la lista de atletas asignados.
        /// Se usa en el detalle, donde la entidad viene con los atletas cargados.
        /// </summary>
        public static EntrenamientoDto ToDto(this Entrenamiento entrenamiento)
        {
            // Los tiempos de K2/K4 cuentan para cada remero del bote.
            var trabajosDeEquipo = entrenamiento.Equipos
                .SelectMany(equipo => equipo.Miembros.Select(miembro => (miembro.AtletaId, equipo.Trabajos)))
                .GroupBy(x => x.AtletaId)
                .ToDictionary(
                    g => g.Key,
                    g => (IReadOnlyList<Trabajo>)g.SelectMany(x => x.Trabajos).ToList());

            var atletas = entrenamiento.Atletas
                .Select(vinculo =>
                {
                    trabajosDeEquipo.TryGetValue(vinculo.AtletaId, out var deBote);
                    return ToDto(vinculo, deBote);
                })
                .OrderBy(a => a.Apellido)
                .ThenBy(a => a.Nombre)
                .ToList();

            return new EntrenamientoDto
            {
                Id = entrenamiento.Id,
                Fecha = entrenamiento.Fecha,
                Turno = entrenamiento.Turno,
                Sesion = entrenamiento.Sesion,
                Tipo = entrenamiento.Tipo,
                Descripcion = entrenamiento.Descripcion,
                Club = entrenamiento.Club,
                FechaCreacion = entrenamiento.FechaCreacion,
                CantidadAtletas = atletas.Count,
                CantidadPresentes = atletas.Count(a => a.Asistio == true),
                CantidadAusentes = atletas.Count(a => a.Asistio == false),
                CantidadSinMarcar = atletas.Count(a => a.Asistio == null),
                Atletas = atletas,
                Equipos = entrenamiento.Equipos
                    .Select(ToDto)
                    .OrderBy(e => e.Etiqueta)
                    .ToList()
            };
        }

        public static EquipoEnEntrenamientoDto ToDto(this EntrenamientoEquipo equipo)
        {
            var miembros = equipo.Miembros
                .OrderBy(m => m.Orden)
                .Select(m => new EquipoMiembroDto
                {
                    AtletaId = m.AtletaId,
                    Nombre = m.Atleta.Nombre,
                    Apellido = m.Atleta.Apellido,
                    Color = m.Atleta.Color,
                    Orden = m.Orden
                })
                .ToList();

            return new EquipoEnEntrenamientoDto
            {
                Id = equipo.Id,
                Tipo = equipo.Tipo,
                EquipoId = equipo.EquipoId,
                Etiqueta = NombresBote.Etiqueta(
                    equipo.Tipo,
                    miembros.Select(m => (m.Nombre, m.Apellido))),
                Miembros = miembros,
                CantidadTrabajos = equipo.Trabajos.Count,
                ResumenTrabajos = ResumenDe(equipo.Trabajos)
            };
        }

        /// <summary>Arma el atleta con su asistencia dentro de la sesion.</summary>
        public static AtletaEnEntrenamientoDto ToDto(
            this EntrenamientoAtleta vinculo,
            IReadOnlyList<Trabajo>? trabajosDeEquipo = null)
        {
            var edad = AtletaMapeos.CalcularEdad(vinculo.Atleta.FechaNacimiento);
            var trabajos = trabajosDeEquipo is { Count: > 0 }
                ? vinculo.Trabajos.Concat(trabajosDeEquipo).ToList()
                : vinculo.Trabajos.ToList();

            return new AtletaEnEntrenamientoDto
            {
                AtletaId = vinculo.AtletaId,
                Nombre = vinculo.Atleta.Nombre,
                Apellido = vinculo.Atleta.Apellido,
                Dni = vinculo.Atleta.Dni,
                Edad = edad,
                Color = vinculo.Atleta.Color,
                Categorias = CategoriasAtleta.Nombres(vinculo.Atleta.FechaNacimiento),
                Asistio = vinculo.Asistio,
                CantidadTrabajos = trabajos.Count,
                ResumenTrabajos = ResumenDe(trabajos),
                Garmin = ActividadDe(vinculo)
            };
        }

        /// <summary>
        /// Resumen de los trabajos cargados, en el orden en que se hicieron.
        /// Un trabajo sin datos cargados no aporta nada, asi que se omite.
        /// </summary>
        private static List<ResumenTrabajoDto> ResumenDe(IEnumerable<Trabajo> trabajos) =>
            trabajos
                .OrderBy(t => t.HoraInicio)
                .ThenBy(t => t.Id)
                .Select(ResumenDe)
                .OfType<ResumenTrabajoDto>()
                .ToList();

        /// <summary>
        /// Arma el resumen de un trabajo. En gimnasio son los kg de cada ejercicio
        /// y en tierra y agua, el tiempo total (suma de parciales). null cuando no
        /// hay nada que mostrar.
        /// </summary>
        private static ResumenTrabajoDto? ResumenDe(Trabajo trabajo)
        {
            if (trabajo.Tipo == TipoTrabajo.Gimnasio)
            {
                var ejercicios = trabajo.Ejercicios
                    .OrderBy(e => e.Orden)
                    .Select(e => new ResumenEjercicioDto
                    {
                        Nombre = e.Nombre,
                        Kilos = e.Series
                            .OrderBy(s => s.Orden)
                            .Where(s => s.PesoKg is not null)
                            .Select(s => s.PesoKg!.Value)
                            .ToList()
                    })
                    .Where(e => e.Kilos.Count > 0)
                    .ToList();

                if (ejercicios.Count == 0)
                {
                    return null;
                }

                return new ResumenTrabajoDto
                {
                    Tipo = trabajo.Tipo,
                    Ejercicios = ejercicios
                };
            }

            // Tierra y agua: en el resumen de la fila va el tiempo total
            // (suma de parciales). El detalle de cada parcial se ve al entrar.
            var parciales = trabajo.Parciales.ToList();
            if (parciales.Count == 0)
            {
                return null;
            }

            var total = TimeSpan.Zero;
            foreach (var parcial in parciales)
            {
                total += parcial.Tiempo;
            }

            return new ResumenTrabajoDto
            {
                Tipo = trabajo.Tipo,
                MejorTiempo = TiempoTexto.Formatear(total)
            };
        }

        /// <summary>Arma el resumen de la actividad solo cuando hay un id de Garmin.</summary>
        private static ActividadGarminDto? ActividadDe(EntrenamientoAtleta vinculo)
        {
            if (vinculo.GarminActividadId is not long actividadId)
            {
                return null;
            }

            return new ActividadGarminDto
            {
                ActividadId = actividadId,
                Nombre = vinculo.GarminNombre ?? "Actividad",
                Tipo = vinculo.GarminTipo,
                Inicio = vinculo.GarminInicio?.ToString("HH:mm"),
                DistanciaMetros = vinculo.GarminDistanciaMetros,
                DuracionSegundos = vinculo.GarminDuracionSegundos,
                FcPromedio = vinculo.GarminFcPromedio,
                FcMaxima = vinculo.GarminFcMaxima,
                Cadencia = vinculo.GarminCadencia,
                Calorias = vinculo.GarminCalorias,
                Url = "https://connect.garmin.com/app/activity/" + actividadId
            };
        }
    }
}
