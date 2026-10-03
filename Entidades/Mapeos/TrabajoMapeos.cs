using Entidades.DTOs;
using Entidades.Util;

namespace Entidades.Mapeos
{
    /// <summary>Conversion de un trabajo y sus parciales a los DTO publicos.</summary>
    public static class TrabajoMapeos
    {
        public static TrabajoDto ToDto(this Trabajo trabajo)
        {
            var ordenados = trabajo.Parciales
                .OrderBy(p => p.Orden)
                .ToList();

            var parciales = new List<ParcialDto>(ordenados.Count);
            TimeSpan? anterior = null;

            foreach (var parcial in ordenados)
            {
                // El parcial es la diferencia con la marca anterior. Si el tiempo
                // no avanzo, se omite para no mostrar un numero enganoso.
                var diferencia = anterior is null ? (TimeSpan?)null : parcial.Tiempo - anterior.Value;

                parciales.Add(new ParcialDto
                {
                    Id = parcial.Id,
                    DistanciaMetros = parcial.DistanciaMetros,
                    Tiempo = TiempoTexto.Formatear(parcial.Tiempo),
                    Parcial = diferencia >= TimeSpan.Zero ? TiempoTexto.Formatear(diferencia.Value) : null,
                    Orden = parcial.Orden
                });

                anterior = parcial.Tiempo;
            }

            // Las muestras de ppm se reparten segun su tiempo: cada una cae en el
            // parcial que le corresponde, y ademas quedan todas juntas en el DTO
            // (que es lo que necesita el formulario para poder editarlas).
            var acumulados = ordenados.Select(p => p.Tiempo).ToList();
            var paladas = new List<PaladaDto>();

            foreach (var palada in trabajo.Paladas.OrderBy(p => p.Orden))
            {
                var dto = new PaladaDto
                {
                    Id = palada.Id,
                    Tiempo = TiempoTexto.Formatear(palada.Tiempo),
                    Ppm = palada.Ppm,
                    Orden = palada.Orden
                };

                paladas.Add(dto);

                if (parciales.Count > 0)
                {
                    parciales[ParcialDe(palada.Tiempo, acumulados)].Paladas.Add(dto);
                }
            }

            // El orden de los ejercicios y de las series dentro de cada uno
            // es el que puso el entrenador al cargarlos.
            var ejercicios = trabajo.Ejercicios
                .OrderBy(e => e.Orden)
                .Select(e => new EjercicioDto
                {
                    Id = e.Id,
                    Nombre = e.Nombre,
                    Orden = e.Orden,
                    Series = e.Series
                        .OrderBy(s => s.Orden)
                        .Select(s => new SerieDto
                        {
                            Id = s.Id,
                            Repeticiones = s.Repeticiones,
                            Porcentaje = s.Porcentaje,
                            PesoKg = s.PesoKg,
                            Orden = s.Orden
                        })
                        .ToList()
                })
                .ToList();

            return new TrabajoDto
            {
                Id = trabajo.Id,
                EntrenamientoId = trabajo.EntrenamientoId,
                AtletaId = trabajo.AtletaId,
                Tipo = trabajo.Tipo,
                HoraInicio = trabajo.HoraInicio is null ? null : TiempoTexto.FormatearHora(trabajo.HoraInicio.Value),
                Observaciones = trabajo.Observaciones,
                FechaCreacion = trabajo.FechaCreacion,
                Parciales = parciales,
                Ejercicios = ejercicios,
                Paladas = paladas,
                EntrenamientoFecha = trabajo.EntrenamientoAtleta?.Entrenamiento?.Fecha,
                EntrenamientoTurno = trabajo.EntrenamientoAtleta?.Entrenamiento?.Turno,
                EntrenamientoSesion = trabajo.EntrenamientoAtleta?.Entrenamiento?.Sesion
            };
        }

        /// <summary>
        /// El parcial al que corresponde una muestra tomada en el tiempo indicado:
        /// el primero cuyo tiempo acumulado sea mayor. Una muestra de 1:40 va al
        /// parcial de 1:45 y una de 15" al de 0:50. Lo que pasa del ultimo parcial
        /// se queda en el ultimo para no perder la muestra.
        /// </summary>
        private static int ParcialDe(TimeSpan tiempo, List<TimeSpan> acumulados)
        {
            for (var i = 0; i < acumulados.Count; i++)
            {
                if (tiempo < acumulados[i])
                {
                    return i;
                }
            }

            return acumulados.Count - 1;
        }
    }
}
