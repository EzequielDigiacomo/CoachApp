namespace Entidades.DTOs
{
    /// <summary>Si el entrenador ya vinculo su cuenta de Garmin.</summary>
    public class EstadoGarminDto
    {
        public bool Vinculada { get; set; }

        /// <summary>Nombre con el que figura la cuenta en Garmin.</summary>
        public string? Nombre { get; set; }
    }
}
