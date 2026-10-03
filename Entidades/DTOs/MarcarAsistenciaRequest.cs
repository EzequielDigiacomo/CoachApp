namespace Entidades.DTOs
{
    /// <summary>
    /// Marca la asistencia de un atleta. Asistio en null borra la marca.
    /// </summary>
    public class MarcarAsistenciaRequest
    {
        public int AtletaId { get; set; }

        public bool? Asistio { get; set; }
    }
}
