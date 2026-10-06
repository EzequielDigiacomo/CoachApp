namespace Entidades.DTOs
{
    /// <summary>
    /// Atleta visto desde un entrenamiento, con su marca de asistencia.
    /// </summary>
    public class AtletaEnEntrenamientoDto
    {
        public int AtletaId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Dni { get; set; } = string.Empty;
        public int Edad { get; set; }

        /// <summary>Nombres de las categorias que cubre esa edad.</summary>
        public List<string> Categorias { get; set; } = [];

        /// <summary>null cuando todavia no se marco la asistencia.</summary>
        public bool? Asistio { get; set; }

        /// <summary>Cantidad de trabajos o controles cargados en la sesion.</summary>
        public int CantidadTrabajos { get; set; }

        /// <summary>Actividad de Garmin del dia, si el nombre coincidio con un amigo.</summary>
        public ActividadGarminDto? Garmin { get; set; }
    }
}
