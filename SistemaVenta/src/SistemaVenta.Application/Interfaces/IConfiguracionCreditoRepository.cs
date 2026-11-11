
using SistemaVenta.Domain.Entities;

namespace SistemaVenta.Application.Interfaces;

public interface IConfiguracionCreditoRepository
{
    Task<decimal> ObtenerLimitePredeterminadoAsync();

    Task<ConfiguracionCredito?> ObtenerConfiguracionAsync();
}
