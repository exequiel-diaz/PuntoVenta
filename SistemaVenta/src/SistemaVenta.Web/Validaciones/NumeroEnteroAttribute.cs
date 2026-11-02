using System.ComponentModel.DataAnnotations;

namespace SistemaVenta.Web.Validaciones;

public class NumeroEnteroAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(
        object? value,
        ValidationContext validationContext)
    {
        if (value is null)
        {
            return ValidationResult.Success;
        }

        if (value is decimal numero &&
            numero != decimal.Truncate(numero))
        {
            return new ValidationResult(
                ErrorMessage ??
                "La cantidad debe ser un número entero.");
        }

        return ValidationResult.Success;
    }
}