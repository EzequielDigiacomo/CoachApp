using Entidades;
using Microsoft.AspNetCore.Identity;

namespace Controladores.Seguridad
{
    /// <summary>Envuelve el hasher de ASP.NET Core para hashear y verificar contrasenas.</summary>
    public class PasswordServicio
    {
        private readonly PasswordHasher<Usuario> _hasher = new();

        public string Hashear(Usuario usuario, string password) =>
            _hasher.HashPassword(usuario, password);

        public bool Verificar(Usuario usuario, string password) =>
            _hasher.VerifyHashedPassword(usuario, usuario.PasswordHash, password)
                != PasswordVerificationResult.Failed;
    }
}
