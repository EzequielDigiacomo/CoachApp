namespace Entidades.DTOs
{
    /// <summary>
    /// Una muestra de paladas por minuto tomada durante un trabajo de agua.
    /// El tiempo es el momento de la toma, contado desde el inicio del trabajo.
    /// </summary>
    public class PaladaDto
    {
        public int Id { get; set; }

        /// <summary>El tiempo tal como se cargo, ya normalizado ("1:40").</summary>
        public string Tiempo { get; set; } = string.Empty;

        /// <summary>Paladas por minuto medidas en ese momento.</summary>
        public int Ppm { get; set; }

        public int Orden { get; set; }
    }
}
