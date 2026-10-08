namespace Entidades.DTOs
{
    /// <summary>Un ejercicio dentro del resumen: su nombre y los kg de sus series.</summary>
    public class ResumenEjercicioDto
    {
        public string Nombre { get; set; } = string.Empty;

        /// <summary>Pesos de cada serie, en orden. No entran las series sin kg.</summary>
        public List<decimal> Kilos { get; set; } = [];
    }
}
