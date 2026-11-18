using System.ComponentModel.DataAnnotations;

namespace Entidades.DTOs
{
    public class AvisoDto
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Texto { get; set; } = string.Empty;
        public string? Link { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
    }

    public class GuardarAvisoRequest
    {
        [Required, MaxLength(120)]
        public string Titulo { get; set; } = string.Empty;

        [Required, MaxLength(1000)]
        public string Texto { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Link { get; set; }

        public bool Activo { get; set; } = true;
    }
}
