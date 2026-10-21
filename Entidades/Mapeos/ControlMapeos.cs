using Entidades.DTOs;

namespace Entidades.Mapeos
{
    public static class ControlMapeos
    {
        public static ControlDto ToDto(this Control control)
        {
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
    }
}
