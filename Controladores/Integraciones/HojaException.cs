namespace Controladores.Integraciones
{
    /// <summary>
    /// Un problema al leer la planilla. El mensaje esta escrito para que lo lea
    /// el entrenador, asi que el controlador lo devuelve tal cual.
    /// </summary>
    public class HojaException : Exception
    {
        public HojaException(string mensaje) : base(mensaje)
        {
        }
    }
}
