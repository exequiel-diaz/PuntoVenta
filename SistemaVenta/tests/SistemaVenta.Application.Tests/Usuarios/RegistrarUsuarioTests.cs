
using SistemaVenta.Application.Tests.Fakes;
using SistemaVenta.Application.UseCases.Usuarios;
using SistemaVenta.Domain.Enums;

namespace SistemaVenta.Application.Tests.Usuarios;

public class RegistrarUsuarioTests
{
    private const string PasswordValida = "PasswordSegura123!";

    [Fact]
    public async Task RegistrarUsuario_ConDatosValidos_DebeGuardarUsuario()
    {
        var repositorio = new FakeUsuarioRepository();
        var hasher = new FakePasswordHasher();
        var unitOfWork = new FakeUnitOfWork();

        var casoDeUso = new RegistrarUsuario(
            repositorio, hasher, unitOfWork);

        await casoDeUso.EjecutarAsync(
            "admin",
            "Administrador General",
            PasswordValida,
            RolUsuario.Administrador);

        var usuario = Assert.Single(repositorio.Usuarios);

        Assert.Equal("admin", usuario.NombreUsuario);
        Assert.Equal("Administrador General", usuario.NombreCompleto);
        Assert.Equal(RolUsuario.Administrador, usuario.Rol);
        Assert.True(usuario.Activo);

        Assert.Equal(
            $"HASH_{PasswordValida}",
            usuario.PasswordHash);

        Assert.Equal(
            PasswordValida,
            hasher.UltimaPasswordRecibida);

        Assert.True(unitOfWork.GuardarCambiosFueLlamado);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task RegistrarUsuario_ConNombreInvalido_DebeRechazarlo(
        string? nombre)
    {
        var repositorio = new FakeUsuarioRepository();
        var unitOfWork = new FakeUnitOfWork();

        var casoDeUso = new RegistrarUsuario(
            repositorio,
            new FakePasswordHasher(),
            unitOfWork);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            casoDeUso.EjecutarAsync(
                nombre!,
                "Administrador General",
                PasswordValida,
                RolUsuario.Administrador));

        Assert.Empty(repositorio.Usuarios);
        Assert.False(unitOfWork.GuardarCambiosFueLlamado);
    }

    [Fact]
    public async Task RegistrarUsuario_ConNombreCompletoVacio_DebeRechazarlo()
    {
        var repositorio = new FakeUsuarioRepository();
        var unitOfWork = new FakeUnitOfWork();

        var casoDeUso = new RegistrarUsuario(
            repositorio,
            new FakePasswordHasher(),
            unitOfWork);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            casoDeUso.EjecutarAsync(
                "admin",
                "",
                PasswordValida,
                RolUsuario.Administrador));

        Assert.Empty(repositorio.Usuarios);
        Assert.False(unitOfWork.GuardarCambiosFueLlamado);
    }

    [Fact]
    public async Task RegistrarUsuario_ConPasswordVacia_DebeRechazarla()
    {
        var repositorio = new FakeUsuarioRepository();
        var unitOfWork = new FakeUnitOfWork();

        var casoDeUso = new RegistrarUsuario(
            repositorio,
            new FakePasswordHasher(),
            unitOfWork);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            casoDeUso.EjecutarAsync(
                "admin",
                "Administrador General",
                "",
                RolUsuario.Administrador));

        Assert.Empty(repositorio.Usuarios);
        Assert.False(unitOfWork.GuardarCambiosFueLlamado);
    }

    [Fact]
    public async Task RegistrarUsuario_ConPasswordCorta_DebeRechazarla()
    {
        var repositorio = new FakeUsuarioRepository();
        var unitOfWork = new FakeUnitOfWork();

        var casoDeUso = new RegistrarUsuario(
            repositorio,
            new FakePasswordHasher(),
            unitOfWork);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            casoDeUso.EjecutarAsync(
                "admin",
                "Administrador General",
                "123456",
                RolUsuario.Administrador));

        Assert.Empty(repositorio.Usuarios);
        Assert.False(unitOfWork.GuardarCambiosFueLlamado);
    }

    [Fact]
    public async Task RegistrarUsuario_ConRolInvalido_DebeRechazarlo()
    {
        var repositorio = new FakeUsuarioRepository();
        var unitOfWork = new FakeUnitOfWork();

        var casoDeUso = new RegistrarUsuario(
            repositorio,
            new FakePasswordHasher(),
            unitOfWork);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            casoDeUso.EjecutarAsync(
                "admin",
                "Administrador General",
                PasswordValida,
                (RolUsuario)999));

        Assert.Empty(repositorio.Usuarios);
        Assert.False(unitOfWork.GuardarCambiosFueLlamado);
    }

    [Fact]
    public async Task RegistrarUsuario_ConNombreDuplicado_DebeRechazarlo()
    {
        var repositorio = new FakeUsuarioRepository();
        var hasher = new FakePasswordHasher();

        var primerRegistro = new RegistrarUsuario(
            repositorio,
            hasher,
            new FakeUnitOfWork());

        await primerRegistro.EjecutarAsync(
            "admin",
            "Primer Administrador",
            PasswordValida,
            RolUsuario.Administrador);

        var segundoUnitOfWork = new FakeUnitOfWork();

        var segundoRegistro = new RegistrarUsuario(
            repositorio,
            hasher,
            segundoUnitOfWork);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            segundoRegistro.EjecutarAsync(
                "ADMIN",
                "Segundo Administrador",
                PasswordValida,
                RolUsuario.Administrador));

        Assert.Single(repositorio.Usuarios);
        Assert.False(segundoUnitOfWork.GuardarCambiosFueLlamado);
    }

    [Fact]
    public async Task RegistrarUsuario_ConEspacios_DebeNormalizarNombre()
    {
        var repositorio = new FakeUsuarioRepository();

        var casoDeUso = new RegistrarUsuario(
            repositorio,
            new FakePasswordHasher(),
            new FakeUnitOfWork());

        await casoDeUso.EjecutarAsync(
            "  vendedor1  ",
            "  Vendedor Principal  ",
            PasswordValida,
            RolUsuario.Vendedor);

        var usuario = Assert.Single(repositorio.Usuarios);

        Assert.Equal("vendedor1", usuario.NombreUsuario);
        Assert.Equal("Vendedor Principal", usuario.NombreCompleto);
    }
}
