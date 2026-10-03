using System.ComponentModel.DataAnnotations;

namespace Entidades
{
    /// <summary>
    /// Atleta que entrena con el coach.
    /// </summary>
    public class Atleta
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Apellido { get; set; } = string.Empty;

        public DateOnly FechaNacimiento { get; set; }

        [MaxLength(150)]
        public string? Club { get; set; }

        [MaxLength(150), EmailAddress]
        public string? Email { get; set; }

        [Required, MaxLength(20)]
        public string Dni { get; set; } = string.Empty;

        [MaxLength(30)]
        public string? Telefono { get; set; }

        public bool Activo { get; set; } = true;

        public DateTime FechaAlta { get; set; } = DateTime.UtcNow;

        /// <summary>Entrenamientos en los que participa el atleta (relacion N:N).</summary>
        public ICollection<EntrenamientoAtleta> Entrenamientos { get; set; } = new List<EntrenamientoAtleta>();

        /// <summary>Anotaciones asociadas al atleta.</summary>
        public ICollection<Anotacion> Anotaciones { get; set; } = new List<Anotacion>();
    }
}
