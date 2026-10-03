namespace Entidades
{
    /// <summary>
    /// Un parcial dentro de un trabajo: la distancia donde se tomo la marca
    /// y el tiempo registrado ahi. Por ejemplo, un control de 1000 m con
    /// parciales a los 250, 500, 750 y 1000 m.
    /// </summary>
    public class TrabajoParcial
    {
        public int Id { get; set; }

        public int TrabajoId { get; set; }
        public Trabajo Trabajo { get; set; } = null!;

        /// <summary>Distancia donde se tomo el parcial, en metros.</summary>
        public int DistanciaMetros { get; set; }

        public TimeSpan Tiempo { get; set; }

        /// <summary>Orden en que se cargaron los parciales dentro del trabajo.</summary>
        public int Orden { get; set; }
    }
}
