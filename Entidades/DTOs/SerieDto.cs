namespace Entidades.DTOs
{
    /// <summary>Una serie de un ejercicio de gimnasio.</summary>
    public class SerieDto
    {
        public int Id { get; set; }

        public int Repeticiones { get; set; }

        public int? Porcentaje { get; set; }

        public decimal? PesoKg { get; set; }

        public int Orden { get; set; }
    }
}
