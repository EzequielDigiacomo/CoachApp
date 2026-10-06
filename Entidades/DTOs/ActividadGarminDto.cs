namespace Entidades.DTOs
{
    /// <summary>
    /// Actividad de un amigo de Garmin asociada al atleta en la sesion.
    /// El enlace abre esa actividad en Garmin Connect.
    /// </summary>
    public class ActividadGarminDto
    {
        public long ActividadId { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string? Tipo { get; set; }

        /// <summary>Hora local de inicio, "08:12".</summary>
        public string? Inicio { get; set; }

        public double? DistanciaMetros { get; set; }

        public double? DuracionSegundos { get; set; }

        public int? FcPromedio { get; set; }

        public int? FcMaxima { get; set; }

        /// <summary>Paladas o revoluciones por minuto, segun el deporte.</summary>
        public double? Cadencia { get; set; }

        public int? Calorias { get; set; }

        public string Url { get; set; } = string.Empty;
    }
}
