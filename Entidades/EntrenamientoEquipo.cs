using Entidades.Enums;

namespace Entidades
{
    /// <summary>
    /// Bote de equipo cargado en una sesion. Puede venir del catalogo
    /// o armarse con los atletas de la sesion.
    /// </summary>
    public class EntrenamientoEquipo
    {
        public int Id { get; set; }

        public int EntrenamientoId { get; set; }
        public Entrenamiento Entrenamiento { get; set; } = null!;

        public TipoBote Tipo { get; set; }

        /// <summary>Si se eligio un bote del catalogo.</summary>
        public int? EquipoId { get; set; }
        public Equipo? Equipo { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public ICollection<EntrenamientoEquipoMiembro> Miembros { get; set; } = new List<EntrenamientoEquipoMiembro>();

        public ICollection<Trabajo> Trabajos { get; set; } = new List<Trabajo>();
    }
}
