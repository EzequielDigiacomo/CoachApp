namespace Entidades.DTOs
{
    /// <summary>Preview de actividades Garmin del dia, sin guardar todavia.</summary>
    public class PreviewGarminDto
    {
        public List<string> Avisos { get; set; } = [];

        /// <summary>True si algun atleta tiene mas de una actividad o mas de una serie.</summary>
        public bool RequiereEleccion { get; set; }

        public List<AtletaPreviewGarminDto> Atletas { get; set; } = [];
    }

    public class AtletaPreviewGarminDto
    {
        public int AtletaId { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Apellido { get; set; } = string.Empty;

        public List<ActividadPreviewGarminDto> Actividades { get; set; } = [];
    }

    public class ActividadPreviewGarminDto
    {
        public long ActividadId { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string? Tipo { get; set; }

        /// <summary>Hora local de inicio, "HH:mm".</summary>
        public string? Inicio { get; set; }

        public double? DistanciaMetros { get; set; }

        public double? DuracionSegundos { get; set; }

        /// <summary>True si el tipo de la actividad encaja con el tipo de la sesion.</summary>
        public bool CoincideTipoSesion { get; set; }

        public List<TramoPreviewGarminDto> Tramos { get; set; } = [];
    }

    public class TramoPreviewGarminDto
    {
        public int Indice { get; set; }

        public int Metros { get; set; }

        /// <summary>Duracion del tramo, ya formateada (m:ss,cc).</summary>
        public string Tiempo { get; set; } = string.Empty;

        public int? Ppm { get; set; }
    }

    /// <summary>Seleccion del entrenador para importar desde el preview.</summary>
    public class ImportarGarminRequest
    {
        public List<ImportarAtletaGarminRequest> Atletas { get; set; } = [];
    }

    public class ImportarAtletaGarminRequest
    {
        public int AtletaId { get; set; }

        public List<ImportarActividadGarminRequest> Actividades { get; set; } = [];
    }

    public class ImportarActividadGarminRequest
    {
        public long ActividadId { get; set; }

        /// <summary>Indices de tramos a guardar. Vacio = todos.</summary>
        public List<int> IndicesTramos { get; set; } = [];
    }
}
