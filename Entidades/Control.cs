using System.ComponentModel.DataAnnotations;
using Entidades.Enums;

namespace Entidades
{
    /// <summary>
    /// Control de fuerza (u otro tipo) independiente de una sesion de entrenamiento.
    /// El detalle de gimnasio vive en ejercicios y series.
    /// </summary>
    public class Control
    {
        public int Id { get; set; }

        public int AtletaId { get; set; }
        public Atleta Atleta { get; set; } = null!;

        /// <summary>Gimnasio, tierra o agua.</summary>
        public TipoTrabajo Tipo { get; set; }

        public DateOnly Fecha { get; set; }

        /// <summary>
        /// Auto carga (peso corporal): solo ejercicio y repeticiones, sin kg ni 1RM.
        /// Solo aplica a controles de gimnasio.
        /// </summary>
        public bool EsAutoCarga { get; set; }

        /// <summary>Distancia del control de agua, en metros.</summary>
        public int? DistanciaMetros { get; set; }

        /// <summary>Tiempo logrado en el control de agua.</summary>
        public TimeSpan? Tiempo { get; set; }

        /// <summary>Tiempo testigo de referencia (del año indicado).</summary>
        public TimeSpan? TiempoTestigo { get; set; }

        /// <summary>Año al que corresponde el tiempo testigo.</summary>
        public int? AnioTestigo { get; set; }

        [MaxLength(300)]
        public string? Observaciones { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Trabajo de sesion desde el que se promovio este control.
        /// Si se borra el trabajo, el control queda (historial) y el vinculo se anula.
        /// </summary>
        public int? TrabajoId { get; set; }
        public Trabajo? Trabajo { get; set; }

        /// <summary>
        /// Parcial concreto (agua/tierra). Unico: un parcial no genera dos controles.
        /// </summary>
        public int? TrabajoParcialId { get; set; }
        public TrabajoParcial? TrabajoParcial { get; set; }

        public ICollection<ControlEjercicio> Ejercicios { get; set; } = new List<ControlEjercicio>();
    }
}
