namespace Entidades.Util
{
    /// <summary>Normaliza pagina/tamano para listados concurrentes sin traer todo.</summary>
    public static class Paginacion
    {
        public const int TamanoDefault = 100;
        public const int TamanoMaximo = 500;

        public static (int Skip, int Take, int Pagina, int Tamano) Normalizar(int? pagina, int? tamano)
        {
            var paginaSegura = pagina is null or < 1 ? 1 : pagina.Value;
            var tamanoSeguro = tamano is null or < 1
                ? TamanoDefault
                : Math.Min(tamano.Value, TamanoMaximo);

            return ((paginaSegura - 1) * tamanoSeguro, tamanoSeguro, paginaSegura, tamanoSeguro);
        }
    }
}
