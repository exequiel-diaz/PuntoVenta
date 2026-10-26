
using SistemaVenta.Infrastructure.Security;

namespace SistemaVenta.Infrastructure.Tests.Security;

public class PasswordHasherServiceTests
{
    private readonly PasswordHasherService _hasher = new();

    [Fact]
    public void GenerarHash_ConPasswordValida_DebeGenerarHash()
    {
        // Arrange
        string password = "PasswordSegura123!";

        // Act
        string hash = _hasher.GenerarHash(password);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(hash));
        Assert.NotEqual(password, hash);
    }

    [Fact]
    public void GenerarHash_DosVeces_DebeGenerarHashesDiferentes()
    {
        // Arrange
        string password = "PasswordSegura123!";

        // Act
        string hash1 = _hasher.GenerarHash(password);
        string hash2 = _hasher.GenerarHash(password);

        // Assert
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void VerificarPassword_ConPasswordCorrecta_DebeDevolverTrue()
    {
        // Arrange
        string password = "PasswordSegura123!";
        string hash = _hasher.GenerarHash(password);

        // Act
        bool resultado = _hasher.VerificarPassword(
            password, hash);

        // Assert
        Assert.True(resultado);
    }

    [Fact]
    public void VerificarPassword_ConPasswordIncorrecta_DebeDevolverFalse()
    {
        // Arrange
        string hash = _hasher.GenerarHash(
            "PasswordSegura123!");

        // Act
        bool resultado = _hasher.VerificarPassword(
            "PasswordIncorrecta123!", hash);

        // Assert
        Assert.False(resultado);
    }

    [Fact]
    public void VerificarPassword_ConHashInvalido_DebeDevolverFalse()
    {
        // Act
        bool resultado = _hasher.VerificarPassword(
            "PasswordSegura123!",
            "hash-invalido");

        // Assert
        Assert.False(resultado);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void GenerarHash_ConPasswordVacia_DebeLanzarExcepcion(
        string? password)
    {
        Assert.Throws<ArgumentException>(() =>
            _hasher.GenerarHash(password!));
    }

    [Theory]
    [InlineData("", "hash")]
    [InlineData("password", "")]
    [InlineData(null, "hash")]
    [InlineData("password", null)]
    public void VerificarPassword_ConDatosInvalidos_DebeDevolverFalse(
        string? password,
        string? hash)
    {
        bool resultado = _hasher.VerificarPassword(
            password!, hash!);

        Assert.False(resultado);
    }
}
