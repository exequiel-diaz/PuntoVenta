using Microsoft.EntityFrameworkCore;
using SistemaVenta.Application.Interfaces;

namespace SistemaVenta.Infrastructure.Persistence;

public class TransaccionCobroVenta : ITransaccionCobroVenta
{
    private readonly AppDbContext _context;

    public TransaccionCobroVenta(AppDbContext context)
    {
        _context = context;
    }

    public async Task<T> EjecutarAsync<T>(
        Func<Task<T>> operacion,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operacion);

        if (_context.Database.CurrentTransaction is not null)
        {
            return await operacion();
        }

        await using var transaccion =
            await _context.Database.BeginTransactionAsync(
                cancellationToken);

        var resultado = await operacion();

        await transaccion.CommitAsync(cancellationToken);

        return resultado;
    }
}