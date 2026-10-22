using Entidades.DTOs;
using Entidades.Util;

namespace Entidades.Mapeos
{
    public static class ControlMapeos
    {
        public static ControlDto ToDto(this Control control)
        {
            string? diferencia = null;
            if (control.Tiempo is not null && control.TiempoTestigo is not null)
            {
                var delta = control.Tiempo.Value - control.TiempoTestigo.Value;
                var signo = delta < TimeSpan.Zero ? "-" : "+";
                diferencia = $"{signo}{TiempoTexto.Formatear(delta.Duration())}";
            }

            return new ControlDto
            {
                Id = control.Id,
                AtletaId = control.AtletaId,
                AtletaNombre = control.Atleta is null
                    ? null
                    : $"{control.Atleta.Apellido}, {control.Atleta.Nombre}",
                AtletaColor = control.Atleta?.Color,
                Tipo = control.Tipo,
                Fecha = control.Fecha,
                EsAutoCarga = control.EsAutoCarga,
                DistanciaMetros = control.DistanciaMetros,
                Tiempo = control.Tiempo is null ? null : TiempoTexto.Formatear(control.Tiempo.Value),
                TiempoTestigo = control.TiempoTestigo is null
                    ? null
                    : TiempoTexto.Formatear(control.TiempoTestigo.Value),
                AnioTestigo = control.AnioTestigo,
                VelocidadKmh = VelocidadDe(control.DistanciaMetros, control.Tiempo),
                Categoria = control.Atleta is null
                    ? null
                    : CategoriasAtleta.Nombres(control.Atleta.FechaNacimiento).FirstOrDefault(),
                DiferenciaTestigo = diferencia,
                Observaciones = control.Observaciones,
                FechaCreacion = control.FechaCreacion,
                Ejercicios = control.Ejercicios
                    .OrderBy(e => e.Orden)
                    .Select(e => new ControlEjercicioDto
                    {
                        Id = e.Id,
                        Nombre = e.Nombre,
                        Orden = e.Orden,
                        Series = e.Series
                            .OrderBy(s => s.Orden)
                            .Select(s => new ControlSerieDto
                            {
                                Id = s.Id,
                                Repeticiones = s.Repeticiones,
                                PesoKg = s.PesoKg,
                                EsUnoRm = s.EsUnoRm,
                                Orden = s.Orden
                            })
                            .ToList()
                    })
                    .ToList()
            };
        }

        /// <summary>
        /// km/h = metros * 3.6 / segundos. Misma regla que en el front.
        /// </summary>
        private static decimal? VelocidadDe(int? metros, TimeSpan? tiempo)
        {
            if (metros is null or <= 0 || tiempo is null || tiempo.Value <= TimeSpan.Zero)
            {
                return null;
            }

            var segundos = (decimal)tiempo.Value.TotalSeconds;
            if (segundos <= 0)
            {
                return null;
            }

            return Math.Round(metros.Value * 3.6m / segundos, 2, MidpointRounding.AwayFromZero);
        }
    }
}
