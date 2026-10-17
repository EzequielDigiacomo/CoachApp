using Entidades.DTOs;
using Entidades.Util;

namespace Entidades.Mapeos
{
    public static class EquipoMapeos
    {
        public static EquipoDto ToDto(this Equipo equipo)
        {
            var miembros = equipo.Miembros
                .OrderBy(m => m.Orden)
                .Select(m => new EquipoMiembroDto
                {
                    AtletaId = m.AtletaId,
                    Nombre = m.Atleta.Nombre,
                    Apellido = m.Atleta.Apellido,
                    Orden = m.Orden
                })
                .ToList();

            return new EquipoDto
            {
                Id = equipo.Id,
                Tipo = equipo.Tipo,
                Activo = equipo.Activo,
                FechaCreacion = equipo.FechaCreacion,
                Miembros = miembros,
                Etiqueta = NombresBote.Etiqueta(
                    equipo.Tipo,
                    miembros.Select(m => (m.Nombre, m.Apellido)))
            };
        }
    }
}
