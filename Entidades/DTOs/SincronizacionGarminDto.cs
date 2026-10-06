namespace Entidades.DTOs
{
    /// <summary>
    /// Resultado de leer las actividades del dia de los amigos de Garmin
    /// y cruzarlas con los atletas de la sesion.
    /// </summary>
    public class SincronizacionGarminDto
    {
        public EntrenamientoDto Entrenamiento { get; set; } = new();

        /// <summary>Avisos para el entrenador: cruces, nombres que no coincidieron.</summary>
        public List<string> Avisos { get; set; } = [];
    }
}
