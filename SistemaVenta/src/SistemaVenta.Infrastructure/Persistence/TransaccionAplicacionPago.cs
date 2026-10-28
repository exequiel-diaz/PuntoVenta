
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
            Func<Task> operacion,
            CancellationToken cancellationToken = default)
        {
            if (documentoComercialId <= 0)
                throw new ArgumentException(
                    "El identificador del documento debe ser válido.");

            // Si ya existe una transacción, reutilizarla.
            if (_context.Database.CurrentTransaction is not null)
            {
                await BloquearDocumentoAsync(
                    documentoComercialId,
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
                cancellationToken);

            await operacion();

            await transaccion.CommitAsync(cancellationToken);
        }

        private async Task BloquearDocumentoAsync(
            int documentoComercialId,
            CancellationToken cancellationToken)
        {
            // Limitar el tiempo de espera por bloqueos.
            await _context.Database.ExecuteSqlRawAsync(
                "SET LOCAL lock_timeout = '10s'",
                cancellationToken);

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
