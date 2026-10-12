
namespace SistemaVenta.Domain.ValueObjects;

public static class NormalizadorDocumento
{
    public static string NormalizarDni(string dni)
    {
        if (string.IsNullOrWhiteSpace(dni))
            throw new ArgumentException(
                "El DNI es obligatorio.");

        var normalizado = dni.Trim()
            .Replace(".", "")
            .Replace(" ", "");

        if (normalizado.Length < 7 ||
            normalizado.Length > 8 ||
            !normalizado.All(char.IsAsciiDigit))
        {
            throw new ArgumentException(
                "El DNI debe contener 7 u 8 dígitos.");
        }

        return normalizado;
    }

    public static string NormalizarCuit(string cuit)
    {
        if (string.IsNullOrWhiteSpace(cuit))
            throw new ArgumentException(
                "El CUIT es obligatorio.");

        var normalizado = cuit.Trim()
            .Replace("-", "")
            .Replace(" ", "");

        if (normalizado.Length != 11 ||
            !normalizado.All(char.IsAsciiDigit))
        {
            throw new ArgumentException(
                "El CUIT debe contener 11 dígitos.");
        }

        return normalizado;
    }
}
