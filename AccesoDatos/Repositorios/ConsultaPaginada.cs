using Entidades.Util;
using Microsoft.EntityFrameworkCore;

namespace AccesoDatos.Repositorios
{
    public readonly record struct ResultadoPaginado<T>(List<T> Items, int Total, int Pagina, int Tamano);

    public static class ConsultaPaginada
    {
        public static async Task<ResultadoPaginado<T>> EjecutarAsync<T>(
            IQueryable<T> consulta,
            int? pagina,
            int? tamano,
            CancellationToken cancelacion = default)
        {
            var (skip, take, paginaN, tamanoN) = Paginacion.Normalizar(pagina, tamano);
            var total = await consulta.CountAsync(cancelacion);
            var items = await consulta.Skip(skip).Take(take).ToListAsync(cancelacion);
            return new ResultadoPaginado<T>(items, total, paginaN, tamanoN);
        }
    }
}
