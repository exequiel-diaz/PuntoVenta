
using Microsoft.EntityFrameworkCore;
using SistemaVenta.Application.Interfaces;
using SistemaVenta.Domain.Entities;
using SistemaVenta.Domain.Enums;
using SistemaVenta.Infrastructure.Persistence;

namespace SistemaVenta.Infrastructure.Repositories
{
    public class DocumentoComercialRepository
        : IDocumentoComercialRepository
    {
        private readonly AppDbContext _context;

        public DocumentoComercialRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DocumentoComercial?> ObtenerPorIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return await _context.DocumentosComerciales
                .FirstOrDefaultAsync(
                    d => d.Id == id,
                    cancellationToken);
        }

        public async Task<decimal> ObtenerTotalAplicadoAsync(
            int documentoComercialId,
            CancellationToken cancellationToken = default)
        {
            return await _context.AplicacionesPago
                .Where(a =>
                    a.DocumentoComercialId == documentoComercialId
                    && a.Pago.Estado != EstadoPago.Anulado)
                .SumAsync(
                    a => a.ImporteAplicado,
                    cancellationToken);
        }

        public async Task<Factura?> ObtenerFacturaPorVentaIdAsync(
            int ventaId,
            CancellationToken cancellationToken = default)
        {
            return await _context.DocumentosComerciales
                .OfType<Factura>()
                .FirstOrDefaultAsync(
                    f => f.VentaId == ventaId,
                    cancellationToken);
        }

        public async Task AgregarAsync(
            DocumentoComercial documento,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(documento);

            await _context.DocumentosComerciales.AddAsync(
                documento,
                cancellationToken);
        }


    }
}
