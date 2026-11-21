using AccesoDatos.Repositorios;
using Microsoft.AspNetCore.Mvc;

namespace Controladores.Seguridad
{
    public static class PaginacionRespuesta
    {
        public const string HeaderTotal = "X-Total-Count";
        public const string HeaderPagina = "X-Page";
        public const string HeaderTamano = "X-Page-Size";

        public static void Exponer<T>(ControllerBase controller, ResultadoPaginado<T> pagina)
        {
            controller.Response.Headers[HeaderTotal] = pagina.Total.ToString();
            controller.Response.Headers[HeaderPagina] = pagina.Pagina.ToString();
            controller.Response.Headers[HeaderTamano] = pagina.Tamano.ToString();
        }
    }
}
