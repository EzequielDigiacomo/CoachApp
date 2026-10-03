namespace Entidades.DTOs
{
    /// <summary>Un ejercicio de un trabajo de gimnasio, con sus series.</summary>
    public class EjercicioDto
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public int Orden { get; set; }

        public List<SerieDto> Series { get; set; } = [];
    }
}
