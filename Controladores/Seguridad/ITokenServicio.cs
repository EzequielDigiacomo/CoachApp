using Entidades;

namespace Controladores.Seguridad
{
    public record TokenGenerado(string Token, DateTime Expira);

    public interface ITokenServicio
    {
        TokenGenerado GenerarToken(Usuario usuario);
    }
}
