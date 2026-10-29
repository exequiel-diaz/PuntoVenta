using Microsoft.EntityFrameworkCore;
using SistemaVenta.Application.Interfaces;

namespace SistemaVenta.Infrastructure.Persistence
{
    public class TransaccionPagoVenta : ITransaccionPagoVenta
    {
        private readonly AppDbContext _context;

        public TransaccionPagoVenta(AppDbContext context)
        {
            _context = context;
        }

        public async Task EjecutarAsync(
            int ventaId,
            int pagoId,
            Func<Task> operacion,
            CancellationToken cancellationToken = default)
        {
            if (ventaId <= 0)
            {
                throw new ArgumentException(
                    "El identificador de la venta debe ser válido.");
            }

            if (pagoId <= 0)
            {
                throw new ArgumentException(
                    "El identificador del pago debe ser válido.");
            }

            ArgumentNullException.ThrowIfNull(operacion);

            // Reutilizar una transacción existente.
            if (_context.Database.CurrentTransaction is not null)
            {
                await BloquearRegistrosAsync(
                    ventaId,
                    pagoId,
                    cancellationToken);

                await operacion();
                return;
            }

            // Crear una nueva transacción.
            await using var transaccion =
                await _context.Database.BeginTransactionAsync(
                    cancellationToken);

            await BloquearRegistrosAsync(
                ventaId,
                pagoId,
                cancellationToken);

            await operacion();

            await transaccion.CommitAsync(cancellationToken);
        }

        private async Task BloquearRegistrosAsync(
            int ventaId,
            int pagoId,
            CancellationToken cancellationToken)
        {
            // Limitar el tiempo de espera por bloqueos.
            await _context.Database.ExecuteSqlRawAsync(
                "SET LOCAL lock_timeout = '10s'",
                cancellationToken);

            // Primero bloquear el pago.
            var pagoEncontrado = await _context.Database
                .SqlQuery<int>(
                    $"""
                    SELECT "Id" AS "Value"
                    FROM "Pagos"
                    WHERE "Id" = {pagoId}
                    FOR UPDATE
                    """)
                .SingleOrDefaultAsync(cancellationToken);

            if (pagoEncontrado == 0)
            {
                throw new InvalidOperationException(
                    "El pago no existe.");
            }

            // Después bloquear la venta.
            var ventaEncontrada = await _context.Database
                .SqlQuery<int>(
                    $"""
                    SELECT "Id" AS "Value"
                    FROM "Ventas"
                    WHERE "Id" = {ventaId}
                    FOR UPDATE
                    """)
                .SingleOrDefaultAsync(cancellationToken);

            if (ventaEncontrada == 0)
            {
                throw new InvalidOperationException(
                    "La venta no existe.");
            }
        }
    }
}