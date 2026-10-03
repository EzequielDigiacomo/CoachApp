using System.Net;
using Entidades.DTOs;
using Microsoft.Extensions.Caching.Memory;

namespace Controladores.Integraciones
{
    /// <summary>
    /// Lee una planilla de Google Sheets bajando el libro en formato Excel.
    /// No usa credenciales: la planilla tiene que estar compartida por enlace
    /// o publicada en la web.
    ///
    /// Se baja el xlsx y no el CSV porque se comprobo que, para libros subidos
    /// desde Excel con muchas pestañas, Google devuelve el CSV vacio mientras
    /// que el xlsx trae todo. Ademas el xlsx es lo unico que permite armar la
    /// lista de pestañas para que el entrenador elija.
    ///
    /// El libro se guarda en memoria un rato: bajarlo pesa varios MB y no
    /// tiene sentido repetirlo al elegir pestaña y al cambiar de pestaña.
    /// </summary>
    public class LectorHojasGoogle : ILectorHojasGoogle
    {
        /// <summary>
        /// Solo se leen planillas de Google. El link lo escribe una persona, y
        /// no queremos que este endpoint sirva para pedir cualquier direccion.
        /// </summary>
        private static readonly string[] HostsPermitidos =
        [
            "docs.google.com",
            "spreadsheets.google.com"
        ];

        /// <summary>Mas que esto no es una planilla de entrenamiento.</summary>
        private const long MaximoBytes = 80_000_000;

        private static readonly TimeSpan Vigencia = TimeSpan.FromMinutes(10);

        private readonly HttpClient _http;
        private readonly IMemoryCache _cache;

        public LectorHojasGoogle(HttpClient http, IMemoryCache cache)
        {
            _http = http;
            _cache = cache;
        }

        public async Task<List<string>> ListarPestanasAsync(string url)
        {
            var libro = await DescargarAsync(url);

            try
            {
                return LectorXlsx.NombresDePestanas(libro);
            }
            catch (LectorXlsx.LibroInvalidoException ex)
            {
                throw new HojaException(ex.Message);
            }
        }

        public async Task<HojaLeidaDto> LeerAsync(string url, string pestana, string? rango)
        {
            var libro = await DescargarAsync(url);

            try
            {
                var recorte = RangoHoja.Parsear(rango);
                var leido = LectorXlsx.LeerPestana(libro, pestana, recorte);

                var hoja = new HojaLeidaDto
                {
                    Pestana = pestana,
                    Columnas = leido.Columnas,
                    ColumnaInicial = leido.ColumnaInicial
                };

                foreach (var (numero, celdas) in leido.Filas)
                {
                    hoja.Filas.Add(new FilaHojaDto
                    {
                        Numero = numero,
                        Celdas = celdas
                    });
                }

                return hoja;
            }
            catch (LectorXlsx.LibroInvalidoException ex)
            {
                throw new HojaException(ex.Message);
            }
        }

        private async Task<byte[]> DescargarAsync(string url)
        {
            var (destino, clave) = ArmarDestino(url);

            if (_cache.TryGetValue(clave, out byte[]? guardado) && guardado is not null)
            {
                return guardado;
            }

            HttpResponseMessage respuesta;
            try
            {
                respuesta = await _http.GetAsync(destino);
            }
            catch (TaskCanceledException)
            {
                throw new HojaException(
                    "Google tardó demasiado en responder. El libro es pesado: probá de nuevo.");
            }
            catch (HttpRequestException)
            {
                throw new HojaException("No se pudo llegar a Google para leer la planilla.");
            }

            using (respuesta)
            {
                Verificar(respuesta);

                var libro = await respuesta.Content.ReadAsByteArrayAsync();

                if (libro.LongLength > MaximoBytes)
                {
                    throw new HojaException("El libro es demasiado grande para leerlo.");
                }

                _cache.Set(clave, libro, Vigencia);
                return libro;
            }
        }

        private static void Verificar(HttpResponseMessage respuesta)
        {
            if (respuesta.StatusCode == HttpStatusCode.NotFound)
            {
                throw new HojaException("No se encontró esa planilla. Revisá que el link esté completo.");
            }

            // 401 y 403 son permisos: es el error mas comun y el mas confuso,
            // asi que se explica que hay que hacer.
            if (respuesta.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            {
                throw new HojaException(
                    "Google no dejó leer la planilla. Compartila con «cualquier persona que tenga el enlace» " +
                    "(queda al instante) o publicala en la web.");
            }

            if (!respuesta.IsSuccessStatusCode)
            {
                throw new HojaException(
                    $"Google respondió {((int)respuesta.StatusCode)} al pedir la planilla.");
            }

            // Si Google contesta HTML, el link no apuntaba a una planilla.
            if (respuesta.Content.Headers.ContentType?.MediaType is "text/html")
            {
                throw new HojaException(
                    "Ese link no devolvió la planilla. Copiá la dirección estando parado en el libro.");
            }
        }

        /// <summary>
        /// Convierte el link que pega el entrenador en el link del libro en
        /// Excel. Acepta el link de la barra de direcciones
        /// (/spreadsheets/d/&lt;id&gt;/) y el que da "Publicar en la web"
        /// (/spreadsheets/d/e/&lt;id&gt;/pub).
        /// Devuelve tambien la clave con la que se guarda el libro en memoria.
        /// </summary>
        private static (string Destino, string Clave) ArmarDestino(string url)
        {
            if (!Uri.TryCreate(url.Trim(), UriKind.Absolute, out var uri))
            {
                throw new HojaException("El link no es válido. Copiá la dirección completa de la planilla.");
            }

            if (uri.Scheme != Uri.UriSchemeHttps)
            {
                throw new HojaException("El link tiene que empezar con https://");
            }

            if (!HostsPermitidos.Contains(uri.Host, StringComparer.OrdinalIgnoreCase))
            {
                throw new HojaException("El link tiene que ser de una planilla de Google (docs.google.com).");
            }

            var partes = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

            // Planilla publicada en la web: /spreadsheets/d/e/<publicada>/...
            if (partes.Length >= 4 && partes[0] == "spreadsheets" && partes[1] == "d" && partes[2] == "e")
            {
                var publicada = partes[3];
                return ($"https://docs.google.com/spreadsheets/d/e/{publicada}/pub?output=xlsx", $"pub:{publicada}");
            }

            // Planilla compartida por enlace: /spreadsheets/d/<id>/...
            if (partes.Length >= 3 && partes[0] == "spreadsheets" && partes[1] == "d")
            {
                var id = partes[2];
                return ($"https://docs.google.com/spreadsheets/d/{id}/export?format=xlsx", $"doc:{id}");
            }

            throw new HojaException("No reconozco ese link como una planilla de Google.");
        }
    }
}
