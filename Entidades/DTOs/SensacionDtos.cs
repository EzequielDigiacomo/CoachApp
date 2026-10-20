using System.ComponentModel.DataAnnotations;

namespace Entidades.DTOs
{
    public class SensacionDto
    {
        public int Id { get; set; }
        public int AtletaId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string? Color { get; set; }
        public string Texto { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
    }

    public class GuardarSensacionRequest
    {
        [Required, MaxLength(1000)]
        public string Texto { get; set; } = string.Empty;
    }
}
