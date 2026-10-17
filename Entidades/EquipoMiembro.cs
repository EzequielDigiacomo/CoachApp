namespace Entidades
{
    public class EquipoMiembro
    {
        public int EquipoId { get; set; }
        public Equipo Equipo { get; set; } = null!;

        public int AtletaId { get; set; }
        public Atleta Atleta { get; set; } = null!;

        /// <summary>Orden en el bote: 1 adelante.</summary>
        public int Orden { get; set; }
    }
}
