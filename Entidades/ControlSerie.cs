namespace Entidades
{
    /// <summary>Serie de un ejercicio de control de gimnasio.</summary>
    public class ControlSerie
    {
        public int Id { get; set; }

        public int ControlEjercicioId { get; set; }
        public ControlEjercicio ControlEjercicio { get; set; } = null!;

        public int Repeticiones { get; set; }

        /// <summary>Peso en kilos. Null en controles de auto carga.</summary>
        public decimal? PesoKg { get; set; }

        /// <summary>
        /// Marca esta serie como el 1RM del ejercicio. Solo puede haber una
        /// por ejercicio; ese peso alimenta la grilla de porcentajes.
        /// No aplica en auto carga.
        /// </summary>
        public bool EsUnoRm { get; set; }

        public int Orden { get; set; }
    }
}
