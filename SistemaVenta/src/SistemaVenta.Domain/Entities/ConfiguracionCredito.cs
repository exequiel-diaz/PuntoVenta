
namespace SistemaVenta.Domain.Entities;

public class ConfiguracionCredito
{
    public int Id { get; private set; }

    public decimal LimitePredeterminado { get; private set; }

    private ConfiguracionCredito() { }

    public ConfiguracionCredito(decimal limitePredeterminado)
    {
        CambiarLimite(limitePredeterminado);
    }

    public void CambiarLimite(decimal nuevoLimite)
    {
        if (nuevoLimite <= 0)
            throw new ArgumentException(
                "El límite predeterminado debe ser mayor que cero.");

        LimitePredeterminado = nuevoLimite;
    }
}
