
using SistemaVenta.Application.Interfaces;

namespace SistemaVenta.Application.Tests.Fakes;

public class FakePasswordHasher : IPasswordHasher
{
    public string? UltimaPasswordRecibida { get; private set; }

    public string GenerarHash(string password)
    {
        UltimaPasswordRecibida = password;

        return $"HASH_{password}";
    }

    public bool VerificarPassword(
        string password,
        string passwordHash)
    {
        return passwordHash == $"HASH_{password}";
    }
}
