using Entidades.DTOs;

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
            var atletas = entrenamiento.Atletas
                .Select(ToDto)
                .OrderBy(a => a.Apellido)
                .ThenBy(a => a.Nombre)
                .ToList();

            return new EntrenamientoDto
            {
                Id = entrenamiento.Id,
                Fecha = entrenamiento.Fecha,
                Turno = entrenamiento.Turno,
                Sesion = entrenamiento.Sesion,
                Descripcion = entrenamiento.Descripcion,
                Club = entrenamiento.Club,
                FechaCreacion = entrenamiento.FechaCreacion,
                CantidadAtletas = atletas.Count,
                CantidadPresentes = atletas.Count(a => a.Asistio == true),
                CantidadAusentes = atletas.Count(a => a.Asistio == false),
                CantidadSinMarcar = atletas.Count(a => a.Asistio == null),
                Atletas = atletas
            };
        }

        /// <summary>Arma el atleta con su asistencia dentro de la sesion.</summary>
        public static AtletaEnEntrenamientoDto ToDto(this EntrenamientoAtleta vinculo)
        {
            var edad = AtletaMapeos.CalcularEdad(vinculo.Atleta.FechaNacimiento);

            return new AtletaEnEntrenamientoDto
            {
                AtletaId = vinculo.AtletaId,
                Nombre = vinculo.Atleta.Nombre,
                Apellido = vinculo.Atleta.Apellido,
                Dni = vinculo.Atleta.Dni,
                Edad = edad,
                Categorias = CategoriasAtleta.Nombres(vinculo.Atleta.FechaNacimiento),
                Asistio = vinculo.Asistio,
                CantidadTrabajos = vinculo.Trabajos.Count,
                Garmin = ActividadDe(vinculo)
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
