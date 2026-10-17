using Entidades.Enums;

namespace Entidades.DTOs
{
    /// <summary>
    /// Resumen corto de un trabajo, para la fila del atleta en la sesion:
    /// los kilos en gimnasio y el tiempo total en tierra y agua.
    /// </summary>
    public class ResumenTrabajoDto
    {
        public TipoTrabajo Tipo { get; set; }

        /// <summary>Gimnasio: los ejercicios con los kg cargados, en orden de carga.</summary>
        public List<ResumenEjercicioDto> Ejercicios { get; set; } = [];

        /// <summary>Tierra y agua: suma de los parciales, ya formateada.</summary>
        public string? MejorTiempo { get; set; }
    }
}
