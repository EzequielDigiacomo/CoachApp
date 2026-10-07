namespace Controladores.Integraciones
{
    /// <summary>Fallo al hablar con Garmin Connect, con un mensaje para el entrenador.</summary>
    public class GarminExcepcion : Exception
    {
        public GarminExcepcion(
            string mensaje,
            bool sesionVencida = false,
            bool limite = false,
            bool prohibido = false) : base(mensaje)
        {
            SesionVencida = sesionVencida;
            Limite = limite;
            Prohibido = prohibido;
        }

        /// <summary>true cuando el token fue rechazado y conviene renovarlo.</summary>
        public bool SesionVencida { get; }

        /// <summary>true cuando Garmin pidio esperar antes de reintentar.</summary>
        public bool Limite { get; }

        /// <summary>true cuando Garmin rechazo la lectura (403) y esa parte se puede omitir.</summary>
        public bool Prohibido { get; }
    }
}
