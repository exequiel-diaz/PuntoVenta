
using Microsoft.AspNetCore.Identity;
using SistemaVenta.Application.Interfaces;
using SistemaVenta.Domain.Entities;

namespace SistemaVenta.Infrastructure.Security;

public class PasswordHasherService : IPasswordHasher
{
    private readonly PasswordHasher<Usuario> _passwordHasher;

    public PasswordHasherService()
    {
        _passwordHasher = new PasswordHasher<Usuario>();
    }

    public string GenerarHash(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException(
                "La contraseña es obligatoria.",
                nameof(password));
        }

        return _passwordHasher.HashPassword(null!, password);
    }

    public bool VerificarPassword(
        string password,
        string passwordHash)
    {
        if (string.IsNullOrEmpty(password) ||
            string.IsNullOrWhiteSpace(passwordHash))
        {
            return false;
        }

        try
        {
            var resultado = _passwordHasher.VerifyHashedPassword(
                null!,
                passwordHash,
                password);

            return resultado != PasswordVerificationResult.Failed;
        }
        catch (FormatException)
        {
            return false;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}
