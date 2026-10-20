using System.ComponentModel.DataAnnotations;

namespace Entidades.DTOs
{
    public class AnotacionAtletaDto
    {
        public int Id { get; set; }
        public int AtletaId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string? Color { get; set; }
        public string Texto { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
    }

    public class GuardarAnotacionAtletaRequest
    {
        [Required, MaxLength(4000)]
        public string Texto { get; set; } = string.Empty;
    }
}
