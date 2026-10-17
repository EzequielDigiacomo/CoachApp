using Entidades.Enums;

namespace Entidades
{
    /// <summary>
    /// Bote armado en el catalogo (K2 o K4) para reutilizar en las sesiones.
    /// </summary>
    public class Equipo
    {
        public int Id { get; set; }

        public TipoBote Tipo { get; set; }

        public bool Activo { get; set; } = true;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public ICollection<EquipoMiembro> Miembros { get; set; } = new List<EquipoMiembro>();
    }
}
