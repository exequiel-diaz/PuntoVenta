
using Microsoft.EntityFrameworkCore;
using SistemaVenta.Application.Interfaces;
using SistemaVenta.Domain.Entities;

namespace SistemaVenta.Infrastructure.Persistence
{
    public class TransaccionAplicacionPago
        : ITransaccionAplicacionPago
    {
        private readonly AppDbContext _context;

        public TransaccionAplicacionPago(AppDbContext context)
        {
            _context = context;
        }


        public async Task EjecutarAsync(
            int documentoComercialId,
            int pagoId,
            Func<Task> operacion,
            CancellationToken cancellationToken = default)
        {
            if (documentoComercialId <= 0)
                throw new ArgumentException(
                    "El identificador del documento debe ser válido.");

            if (pagoId <= 0)
                throw new ArgumentException(
                    "El identificador del pago debe ser válido.");

            ArgumentNullException.ThrowIfNull(operacion);

            // Si ya existe una transacción, reutilizarla.
            if (_context.Database.CurrentTransaction is not null)
            {
                await BloquearDocumentoAsync(
                    documentoComercialId,
                    pagoId,
                    cancellationToken);

                await operacion();
                return;
            }

            // Si no existe una transacción, crear una nueva.
            await using var transaccion =
                await _context.Database.BeginTransactionAsync(
                    cancellationToken);

            await BloquearDocumentoAsync(
                documentoComercialId,
                pagoId,
                cancellationToken);

            await operacion();

            await transaccion.CommitAsync(cancellationToken);
        }

        private async Task BloquearDocumentoAsync(
            int documentoComercialId,
            int pagoId,
            CancellationToken cancellationToken)
        {
            // Limitar el tiempo de espera por bloqueos.
            await _context.Database.ExecuteSqlRawAsync(
                "SET LOCAL lock_timeout = '10s'",
                cancellationToken);

            // Bloquear primero el pago para coordinar
            // las aplicaciones a facturas y ventas.
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
                throw new InvalidOperationException(
                    "El pago no existe.");

            // Bloquear la fila del documento comercial.
            var documentoId = await _context.Database
                .SqlQuery<int>(
                    $"""
                    SELECT "Id" AS "Value"
                    FROM "DocumentosComerciales"
                    WHERE "Id" = {documentoComercialId}
                    FOR UPDATE
                    """)
                .SingleOrDefaultAsync(cancellationToken);

            if (documentoId == 0)
                throw new InvalidOperationException(
                    "El documento comercial no existe.");
        }


    }
}
