using System.ComponentModel.DataAnnotations;

namespace Entidades
{
    /// <summary>
    /// Tabla intermedia de la relacion N:N entre entrenamientos y atletas.
    /// </summary>
    public class EntrenamientoAtleta
    {
        public int EntrenamientoId { get; set; }
        public Entrenamiento Entrenamiento { get; set; } = null!;

        public int AtletaId { get; set; }
        public Atleta Atleta { get; set; } = null!;

        /// <summary>Marca de asistencia a la sesion (null = sin marcar).</summary>
        public bool? Asistio { get; set; }

        /// <summary>Actividad de Garmin del amigo, el dia de esta sesion.</summary>
        public long? GarminActividadId { get; set; }

        [MaxLength(200)]
        public string? GarminNombre { get; set; }

        [MaxLength(80)]
        public string? GarminTipo { get; set; }

        /// <summary>Hora local en la que empezo la actividad en Garmin.</summary>
        public DateTime? GarminInicio { get; set; }

        public double? GarminDistanciaMetros { get; set; }

        public double? GarminDuracionSegundos { get; set; }

        public int? GarminFcPromedio { get; set; }

        public int? GarminFcMaxima { get; set; }

        public double? GarminCadencia { get; set; }

        public int? GarminCalorias { get; set; }

        /// <summary>Trabajos o controles que hizo el atleta en esta sesion.</summary>
        public ICollection<Trabajo> Trabajos { get; set; } = new List<Trabajo>();
    }
}
