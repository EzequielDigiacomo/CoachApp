using System.Security.Claims;
using Controladores.Seguridad;
using Entidades.Enums;

namespace CoachApp.Tests.Seguridad;

public class IdentidadTests
{
    [Fact]
    public void EsStaff_SuperAdmin_y_Entrenador()
    {
        Assert.True(Identidad.EsStaff(UsuarioConRol(RolUsuario.SuperAdmin)));
        Assert.True(Identidad.EsStaff(UsuarioConRol(RolUsuario.Entrenador)));
        Assert.False(Identidad.EsStaff(UsuarioConRol(RolUsuario.Atleta)));
    }

    [Fact]
    public void EsAtleta_solo_rol_atleta()
    {
        Assert.True(Identidad.EsAtleta(UsuarioConRol(RolUsuario.Atleta)));
        Assert.False(Identidad.EsAtleta(UsuarioConRol(RolUsuario.Entrenador)));
    }

    [Fact]
    public void AtletaId_lee_claim_custom()
    {
        var usuario = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.Role, nameof(RolUsuario.Atleta)),
            new Claim(Identidad.ClaimAtletaId, "42")
        ], "test"));

        Assert.Equal(42, Identidad.AtletaId(usuario));
    }

    [Fact]
    public void UsuarioId_lee_NameIdentifier()
    {
        var usuario = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, "7")
        ], "test"));

        Assert.Equal(7, Identidad.UsuarioId(usuario));
    }

    private static ClaimsPrincipal UsuarioConRol(RolUsuario rol) =>
        new(new ClaimsIdentity([new Claim(ClaimTypes.Role, rol.ToString())], "test"));
}
