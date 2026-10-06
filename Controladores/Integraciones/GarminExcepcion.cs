namespace Controladores.Integraciones
{
    /// <summary>Fallo al hablar con Garmin Connect, con un mensaje para el entrenador.</summary>
    public class GarminExcepcion : Exception
    {
        public GarminExcepcion(string mensaje, bool sesionVencida = false, bool limite = false) : base(mensaje)
        {
            SesionVencida = sesionVencida;
            Limite = limite;
        }

        /// <summary>true cuando el token fue rechazado y conviene renovarlo.</summary>
        public bool SesionVencida { get; }

        /// <summary>true cuando Garmin pidio esperar antes de reintentar.</summary>
        public bool Limite { get; }
    }
}
