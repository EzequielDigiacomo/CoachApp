namespace Entidades.DTOs
{
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;

        public DateTime Expira { get; set; }

        public UsuarioDto Usuario { get; set; } = null!;
    }
}
