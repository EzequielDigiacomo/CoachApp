namespace Entidades
{
    public class EntrenamientoEquipoMiembro
    {
        public int EntrenamientoEquipoId { get; set; }
        public EntrenamientoEquipo EntrenamientoEquipo { get; set; } = null!;

        public int AtletaId { get; set; }
        public Atleta Atleta { get; set; } = null!;

        public int Orden { get; set; }
    }
}
