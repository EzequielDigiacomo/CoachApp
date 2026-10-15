using Entidades;

namespace AccesoDatos.Repositorios
{
    public interface ICuentaGarminRepositorio
    {
        Task<CuentaGarmin?> ObtenerPorUsuarioAsync(int usuarioId);

        /// <summary>Cualquier cuenta ya vinculada, para el calendario del atleta.</summary>
        Task<CuentaGarmin?> ObtenerAlgunaAsync();

        Task GuardarAsync(CuentaGarmin cuenta);

        Task EliminarAsync(CuentaGarmin cuenta);
    }
}
