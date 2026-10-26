
using SistemaVenta.Application.Tests.Fakes;
using SistemaVenta.Application.UseCases.Usuarios;
using SistemaVenta.Domain.Entities;
using SistemaVenta.Domain.Enums;

namespace SistemaVenta.Application.Tests.Usuarios;

public class AutenticarUsuarioTests
{
    private const string PasswordValida = "PasswordSegura123!";

    private static async Task<(
        FakeUsuarioRepository Repositorio,
        FakePasswordHasher Hasher,
        Usuario Usuario)> PrepararUsuarioAsync()
    {
        var repositorio = new FakeUsuarioRepository();
        var hasher = new FakePasswordHasher();

        var usuario = new Usuario(
            "admin",
            "Administrador General",
            hasher.GenerarHash(PasswordValida),
            RolUsuario.Administrador);

        await repositorio.AgregarAsync(usuario);

        return (repositorio, hasher, usuario);
    }

    [Fact]
    public async Task Autenticar_ConCredencialesValidas_DebeDevolverUsuario()
    {
        var (repositorio, hasher, usuario) =
            await PrepararUsuarioAsync();

        var casoDeUso = new AutenticarUsuario(
            repositorio, hasher);

        var resultado = await casoDeUso.EjecutarAsync(
            "admin", PasswordValida);

        Assert.NotNull(resultado);
        Assert.Same(usuario, resultado);
        Assert.True(resultado.Activo);
        Assert.Equal(RolUsuario.Administrador, resultado.Rol);
    }

    [Fact]
    public async Task Autenticar_ConPasswordIncorrecta_DebeDevolverNull()
    {
        var (repositorio, hasher, _) =
            await PrepararUsuarioAsync();

        var casoDeUso = new AutenticarUsuario(
            repositorio, hasher);

        var resultado = await casoDeUso.EjecutarAsync(
            "admin", "PasswordIncorrecta!");

        Assert.Null(resultado);
    }

    [Fact]
    public async Task Autenticar_ConUsuarioInexistente_DebeDevolverNull()
    {
        var casoDeUso = new AutenticarUsuario(
            new FakeUsuarioRepository(),
            new FakePasswordHasher());

        var resultado = await casoDeUso.EjecutarAsync(
            "desconocido", PasswordValida);

        Assert.Null(resultado);
    }

    [Fact]
    public async Task Autenticar_ConUsuarioInactivo_DebeDevolverNull()
    {
        var (repositorio, hasher, usuario) =
            await PrepararUsuarioAsync();

        usuario.Desactivar();

        var casoDeUso = new AutenticarUsuario(
            repositorio, hasher);

        var resultado = await casoDeUso.EjecutarAsync(
            "admin", PasswordValida);

        Assert.Null(resultado);
    }

    [Theory]
    [InlineData("", "PasswordSegura123!")]
    [InlineData(" ", "PasswordSegura123!")]
    [InlineData(null, "PasswordSegura123!")]
    [InlineData("admin", "")]
    [InlineData("admin", " ")]
    [InlineData("admin", null)]
    public async Task Autenticar_ConCredencialesVacias_DebeDevolverNull(
        string? nombreUsuario,
        string? password)
    {
        var (repositorio, hasher, _) =
            await PrepararUsuarioAsync();

        var casoDeUso = new AutenticarUsuario(
            repositorio, hasher);

        var resultado = await casoDeUso.EjecutarAsync(
            nombreUsuario!,
            password!);

        Assert.Null(resultado);
    }

    [Fact]
    public async Task Autenticar_ConNombreEnMayusculas_DebeAutenticar()
    {
        var (repositorio, hasher, _) =
            await PrepararUsuarioAsync();

        var casoDeUso = new AutenticarUsuario(
            repositorio, hasher);

        var resultado = await casoDeUso.EjecutarAsync(
            "ADMIN", PasswordValida);

        Assert.NotNull(resultado);
        Assert.Equal("admin", resultado.NombreUsuario);
    }

    [Fact]
    public async Task Autenticar_ConEspaciosEnNombre_DebeAutenticar()
    {
        var (repositorio, hasher, _) =
            await PrepararUsuarioAsync();

        var casoDeUso = new AutenticarUsuario(
            repositorio, hasher);

        var resultado = await casoDeUso.EjecutarAsync(
            "  admin  ", PasswordValida);

        Assert.NotNull(resultado);
        Assert.Equal("admin", resultado.NombreUsuario);
    }
}
