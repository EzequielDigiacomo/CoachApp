using AccesoDatos.Repositorios;
using Controladores;
using Controladores.Seguridad;
using Entidades;
using Entidades.DTOs;
using Entidades.Enums;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CoachApp.Tests.Auth;

public class AuthControllerTests
{
    [Fact]
    public async Task Login_credenciales_invalidas_devuelve_401()
    {
        var usuarios = new Mock<IUsuarioRepositorio>();
        usuarios.Setup(u => u.ObtenerPorNombreUsuarioAsync("nadie"))
            .ReturnsAsync((Usuario?)null);

        var controller = new AuthController(
            usuarios.Object,
            new PasswordServicio(),
            Mock.Of<ITokenServicio>());

        var resultado = await controller.Login(new LoginRequest
        {
            NombreUsuario = "nadie",
            Password = "x"
        });

        Assert.IsType<UnauthorizedObjectResult>(resultado.Result);
    }

    [Fact]
    public async Task Login_ok_devuelve_token()
    {
        var usuario = new Usuario
        {
            Id = 1,
            NombreUsuario = "coach",
            Email = "c@test.local",
            Nombre = "Coach",
            Apellido = "Uno",
            Rol = RolUsuario.Entrenador,
            Activo = true,
            FechaCreacion = DateTime.UtcNow
        };
        var passwords = new PasswordServicio();
        usuario.PasswordHash = passwords.Hashear(usuario, "Secreta123!");

        var usuarios = new Mock<IUsuarioRepositorio>();
        usuarios.Setup(u => u.ObtenerPorNombreUsuarioAsync("coach")).ReturnsAsync(usuario);
        usuarios.Setup(u => u.ActualizarAsync(It.IsAny<Usuario>())).Returns(Task.CompletedTask);

        var tokens = new Mock<ITokenServicio>();
        tokens.Setup(t => t.GenerarToken(usuario))
            .Returns(new TokenGenerado("jwt-de-prueba", DateTime.UtcNow.AddHours(1)));

        var controller = new AuthController(usuarios.Object, passwords, tokens.Object);

        var resultado = await controller.Login(new LoginRequest
        {
            NombreUsuario = "coach",
            Password = "Secreta123!"
        });

        var ok = Assert.IsType<OkObjectResult>(resultado.Result);
        var body = Assert.IsType<LoginResponse>(ok.Value);
        Assert.Equal("jwt-de-prueba", body.Token);
        Assert.Equal("coach", body.Usuario.NombreUsuario);
    }
}
