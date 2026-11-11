
using Microsoft.EntityFrameworkCore;
using SistemaVenta.Application.Interfaces;
using SistemaVenta.Domain.Entities;

namespace SistemaVenta.Infrastructure.Persistence.Repositories;

public class ConfiguracionCreditoRepository
    : IConfiguracionCreditoRepository
{
    private readonly AppDbContext _context;

    public ConfiguracionCreditoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<decimal> ObtenerLimitePredeterminadoAsync()
    {
        var limite = await _context.ConfiguracionesCredito
            .AsNoTracking()
            .Where(c => c.Id == 1)
            .Select(c => (decimal?)c.LimitePredeterminado)
            .SingleOrDefaultAsync();

        if (limite is null || limite <= 0)
            throw new InvalidOperationException(
                "No existe una configuración válida del límite de crédito.");

        return limite.Value;
    }

    public async Task<ConfiguracionCredito?>
        ObtenerConfiguracionAsync()
    {
        return await _context.ConfiguracionesCredito
            .SingleOrDefaultAsync(c => c.Id == 1);
    }
}
