using Entidades.Enums;

namespace Entidades.DTOs
{
    /// <summary>
    /// Resumen corto de un trabajo, para la fila del atleta en la sesion:
    /// los kilos en gimnasio y la mejor marca en tierra y agua.
    /// </summary>
    public class ResumenTrabajoDto
    {
        public TipoTrabajo Tipo { get; set; }

        /// <summary>Gimnasio: los ejercicios con los kg cargados, en orden de carga.</summary>
        public List<ResumenEjercicioDto> Ejercicios { get; set; } = [];

        /// <summary>Tierra y agua: el mejor tiempo de los parciales, ya formateado.</summary>
        public string? MejorTiempo { get; set; }
    }
}
