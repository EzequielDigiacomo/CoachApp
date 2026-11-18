using System.ComponentModel.DataAnnotations;

namespace Entidades
{
    /// <summary>
    /// Aviso del cuerpo técnico hacia los atletas (mensaje corto con link opcional).
    /// </summary>
    public class Aviso
    {
        public int Id { get; set; }

        [Required, MaxLength(120)]
        public string Titulo { get; set; } = string.Empty;

        [Required, MaxLength(1000)]
        public string Texto { get; set; } = string.Empty;

        /// <summary>URL opcional (planilla, drive, etc.).</summary>
        [MaxLength(500)]
        public string? Link { get; set; }

        /// <summary>Si es false, el atleta no lo ve; el staff sí lo administra.</summary>
        public bool Activo { get; set; } = true;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    }
}
