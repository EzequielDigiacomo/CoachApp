namespace Entidades
{
    /// <summary>
    /// Una muestra de paladas por minuto (ppm) tomada durante un trabajo de agua.
    /// El tiempo es absoluto, contado desde el inicio del trabajo, y no se guarda
    /// a que parcial pertenece: eso se deduce del tiempo al armar la planilla,
    /// asi la muestra no se rompe si despues se corrige un parcial.
    /// </summary>
    public class TrabajoPalada
    {
        public int Id { get; set; }

        public int TrabajoId { get; set; }
        public Trabajo Trabajo { get; set; } = null!;

        /// <summary>Momento de la toma, contado desde el inicio del trabajo.</summary>
        public TimeSpan Tiempo { get; set; }

        /// <summary>Paladas por minuto medidas en ese momento.</summary>
        public int Ppm { get; set; }

        /// <summary>Orden en que se cargaron las muestras dentro del trabajo.</summary>
        public int Orden { get; set; }
    }
}
