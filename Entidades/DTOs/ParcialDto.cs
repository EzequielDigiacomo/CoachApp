namespace Entidades.DTOs
{
    /// <summary>Un parcial de un trabajo: donde se tomo la marca y su tiempo.</summary>
    public class ParcialDto
    {
        public int Id { get; set; }

        public int DistanciaMetros { get; set; }

        /// <summary>El tiempo tal como se cargo, ya normalizado para mostrar.</summary>
        public string Tiempo { get; set; } = string.Empty;

        /// <summary>
        /// Diferencia con el parcial anterior. Es solo una referencia para leer
        /// la planilla: null en el primero o cuando el tiempo no avanzo.
        /// </summary>
        public string? Parcial { get; set; }

        /// <summary>
        /// Muestras de paladas por minuto que cayeron dentro de este parcial.
        /// El cronometro sigue de corrido: el tramo va desde la suma de las
        /// duraciones anteriores hasta sumarle la de este.
        /// </summary>
        public List<PaladaDto> Paladas { get; set; } = [];

        public int Orden { get; set; }
    }
}
