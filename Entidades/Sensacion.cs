using System.ComponentModel.DataAnnotations;

namespace Entidades
{
    /// <summary>Lo que el atleta sintio al remar. Tope de 1000 caracteres.</summary>
    public class Sensacion
    {
        public int Id { get; set; }

        public int AtletaId { get; set; }
        public Atleta? Atleta { get; set; }

        [Required, MaxLength(1000)]
        public string Texto { get; set; } = string.Empty;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    }
}
