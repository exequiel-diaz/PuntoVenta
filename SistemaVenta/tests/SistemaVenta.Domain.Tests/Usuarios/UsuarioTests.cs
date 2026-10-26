
using SistemaVenta.Domain.Entities;
using SistemaVenta.Domain.Enums;

namespace SistemaVenta.Domain.Tests.Usuarios;

public class UsuarioTests
{
    [Fact]
    public void CrearUsuario_ConDatosValidos_DebeCrearUsuarioActivo()
    {
        var usuario = new Usuario(
            "admin",
            "Administrador General",
            "hash_seguro",
            RolUsuario.Administrador);

        Assert.Equal("admin", usuario.NombreUsuario);
        Assert.Equal("Administrador General", usuario.NombreCompleto);
        Assert.Equal("hash_seguro", usuario.PasswordHash);
        Assert.Equal(RolUsuario.Administrador, usuario.Rol);
        Assert.True(usuario.Activo);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void CrearUsuario_ConNombreUsuarioInvalido_DebeLanzarExcepcion(
        string? nombreUsuario)
    {
        Assert.Throws<ArgumentException>(() =>
            new Usuario(
                nombreUsuario!,
                "Administrador General",
                "hash_seguro",
                RolUsuario.Administrador));
    }

    [Fact]
    public void CrearUsuario_ConNombreCompletoVacio_DebeLanzarExcepcion()
    {
        Assert.Throws<ArgumentException>(() =>
            new Usuario(
                "admin",
                "",
                "hash_seguro",
                RolUsuario.Administrador));
    }

    [Fact]
    public void CrearUsuario_ConHashVacio_DebeLanzarExcepcion()
    {
        Assert.Throws<ArgumentException>(() =>
            new Usuario(
                "admin",
                "Administrador General",
                "",
                RolUsuario.Administrador));
    }

    [Fact]
    public void CrearUsuario_ConRolInvalido_DebeLanzarExcepcion()
    {
        Assert.Throws<ArgumentException>(() =>
            new Usuario(
                "admin",
                "Administrador General",
                "hash_seguro",
                (RolUsuario)999));
    }

    [Fact]
    public void Desactivar_UsuarioActivo_DebeDesactivarlo()
    {
        var usuario = CrearUsuarioValido();

        usuario.Desactivar();

        Assert.False(usuario.Activo);
    }

    [Fact]
    public void Desactivar_UsuarioInactivo_DebeLanzarExcepcion()
    {
        var usuario = CrearUsuarioValido();
        usuario.Desactivar();

        Assert.Throws<InvalidOperationException>(
            () => usuario.Desactivar());
    }

    [Fact]
    public void Activar_UsuarioInactivo_DebeActivarlo()
    {
        var usuario = CrearUsuarioValido();
        usuario.Desactivar();

        usuario.Activar();

        Assert.True(usuario.Activo);
    }

    [Fact]
    public void Activar_UsuarioActivo_DebeLanzarExcepcion()
    {
        var usuario = CrearUsuarioValido();

        Assert.Throws<InvalidOperationException>(
            () => usuario.Activar());
    }

    [Fact]
    public void CambiarPasswordHash_ConHashValido_DebeActualizarlo()
    {
        var usuario = CrearUsuarioValido();

        usuario.CambiarPasswordHash("nuevo_hash_seguro");

        Assert.Equal("nuevo_hash_seguro", usuario.PasswordHash);
    }

    [Fact]
    public void CambiarPasswordHash_ConHashVacio_DebeLanzarExcepcion()
    {
        var usuario = CrearUsuarioValido();

        Assert.Throws<ArgumentException>(() =>
            usuario.CambiarPasswordHash(""));
    }

    private static Usuario CrearUsuarioValido()
    {
        return new Usuario(
            "admin",
            "Administrador General",
            "hash_seguro",
            RolUsuario.Administrador);
    }
}
