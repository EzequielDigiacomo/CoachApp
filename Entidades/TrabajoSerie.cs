namespace Entidades
{
    /// <summary>
    /// Una serie de un ejercicio de gimnasio.
    /// </summary>
    public class TrabajoSerie
    {
        public int Id { get; set; }

        public int TrabajoEjercicioId { get; set; }
        public TrabajoEjercicio TrabajoEjercicio { get; set; } = null!;

        public int Repeticiones { get; set; }

        /// <summary>Porcentaje cargado a mano. No se calcula nada con el.</summary>
        public int? Porcentaje { get; set; }

        /// <summary>Peso en kilos. Queda vacio cuando la serie es a peso corporal.</summary>
        public decimal? PesoKg { get; set; }

        /// <summary>Orden en que se cargaron las series dentro del ejercicio.</summary>
        public int Orden { get; set; }
    }
}
