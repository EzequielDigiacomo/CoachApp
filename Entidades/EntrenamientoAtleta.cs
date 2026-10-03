namespace Entidades
{
    /// <summary>
    /// Tabla intermedia de la relacion N:N entre entrenamientos y atletas.
    /// </summary>
    public class EntrenamientoAtleta
    {
        public int EntrenamientoId { get; set; }
        public Entrenamiento Entrenamiento { get; set; } = null!;

        public int AtletaId { get; set; }
        public Atleta Atleta { get; set; } = null!;

        /// <summary>Marca de asistencia a la sesion (null = sin marcar).</summary>
        public bool? Asistio { get; set; }

        /// <summary>Trabajos o controles que hizo el atleta en esta sesion.</summary>
        public ICollection<Trabajo> Trabajos { get; set; } = new List<Trabajo>();
    }
}
