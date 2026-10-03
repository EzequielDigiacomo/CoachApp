using System.ComponentModel.DataAnnotations;
using Entidades.Enums;

namespace Entidades
{
    /// <summary>
    /// Un trabajo o control que hizo un atleta dentro de una sesion
    /// (por ejemplo, un control de 1000 m). El detalle va en los parciales:
    /// la distancia y el tiempo de cada marca se cargan ahi, en orden de toma.
    /// </summary>
    public class Trabajo
    {
        public int Id { get; set; }

        public int EntrenamientoId { get; set; }
        public int AtletaId { get; set; }

        /// <summary>
        /// El trabajo cuelga del atleta dentro de la sesion, asi que solo existe
        /// si el atleta esta asignado a ese entrenamiento.
        /// </summary>
        public EntrenamientoAtleta EntrenamientoAtleta { get; set; } = null!;

        /// <summary>Lugar donde se hizo el trabajo: gimnasio, tierra o agua.</summary>
        public TipoTrabajo Tipo { get; set; }

        /// <summary>Hora del dia en que se hizo el trabajo.</summary>
        public TimeOnly? HoraInicio { get; set; }

        [MaxLength(300)]
        public string? Observaciones { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Marcas tomadas con el cronometro, en el orden en que se tomaron:
        /// la primera es la 1. Se usan en los trabajos de tierra y agua.
        /// </summary>
        public ICollection<TrabajoParcial> Parciales { get; set; } = new List<TrabajoParcial>();

        /// <summary>
        /// Ejercicios con sus series. Se usan en los trabajos de gimnasio.
        /// </summary>
        public ICollection<TrabajoEjercicio> Ejercicios { get; set; } = new List<TrabajoEjercicio>();

        /// <summary>
        /// Muestras de paladas por minuto (ppm) tomadas durante el trabajo.
        /// Se usan en los trabajos de agua y se reparten por tiempo en la planilla.
        /// </summary>
        public ICollection<TrabajoPalada> Paladas { get; set; } = new List<TrabajoPalada>();

        /// <summary>Anotaciones escritas sobre este trabajo.</summary>
        public ICollection<Anotacion> Anotaciones { get; set; } = new List<Anotacion>();
    }
}
