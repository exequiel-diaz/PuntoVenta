
using Microsoft.EntityFrameworkCore;
using SistemaVenta.Application.Interfaces;
using SistemaVenta.Domain.Entities;

namespace SistemaVenta.Infrastructure.Persistence
{
    public class TransaccionVentaPostgres : ITransaccionVenta
    {
        private readonly AppDbContext _context;

        public TransaccionVentaPostgres(AppDbContext context)
        {
            _context = context;
        }

        public async Task EjecutarAsync(
            int ventaId,
            Func<Task> operacion,
            CancellationToken cancellationToken = default)
        {
            if (ventaId <= 0)
                throw new ArgumentException(
                    "El identificador de la venta debe ser válido.");

            // Reutilizar una transacción existente.
            if (_context.Database.CurrentTransaction is not null)
            {
                await BloquearVentaAsync(
                    ventaId,
                    cancellationToken);

                await operacion();
                return;
            }

            // Crear una nueva transacción.
            await using var transaccion =
                await _context.Database.BeginTransactionAsync(
                    cancellationToken);

            await BloquearVentaAsync(
                ventaId,
                cancellationToken);

            await operacion();

            await transaccion.CommitAsync(cancellationToken);
        }

        private async Task BloquearVentaAsync(
            int ventaId,
            CancellationToken cancellationToken)
        {
            // Evitar esperas indefinidas por bloqueos.
            await _context.Database.ExecuteSqlRawAsync(
                "SET LOCAL lock_timeout = '10s'",
                cancellationToken);

            // Bloquear la fila de la venta.
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
                throw new InvalidOperationException(
                    "La venta no existe.");

            // Si la venta ya estaba cargada en este DbContext,
            // actualizarla con los valores actuales de PostgreSQL.
            var ventaRastreada = _context.ChangeTracker
                .Entries<Venta>()
                .FirstOrDefault(e => e.Entity.Id == ventaId);

            if (ventaRastreada is not null)
            {
                await ventaRastreada.ReloadAsync(cancellationToken);
            }
        }
    }
}
