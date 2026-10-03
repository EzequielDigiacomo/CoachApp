using Entidades.DTOs;

namespace Controladores.Integraciones
{
    /// <summary>Lee una planilla de Google Sheets compartida por link.</summary>
    public interface ILectorHojasGoogle
    {
        /// <summary>
        /// Los nombres de las pestañas del libro, en orden. Es lo que le
        /// permite al entrenador elegir en un libro con muchas pestañas.
        /// </summary>
        Task<List<string>> ListarPestanasAsync(string url);

        /// <summary>
        /// El contenido de una pestaña, opcionalmente recortado a un rango de
        /// celdas. Lanza <see cref="HojaException"/> con un mensaje entendible
        /// si no se puede leer.
        /// </summary>
        Task<HojaLeidaDto> LeerAsync(string url, string pestana, string? rango);
    }
}
