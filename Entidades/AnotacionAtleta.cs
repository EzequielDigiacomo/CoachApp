using System.ComponentModel.DataAnnotations;

namespace Entidades
{
    /// <summary>
    /// Nota que escribe el atleta. El entrenador la lee y no la mezcla
    /// con las anotaciones del cuerpo tecnico.
    /// </summary>
    public class AnotacionAtleta
    {
        public int Id { get; set; }

        public int AtletaId { get; set; }
        public Atleta? Atleta { get; set; }

        [Required, MaxLength(4000)]
        public string Texto { get; set; } = string.Empty;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    }
}
