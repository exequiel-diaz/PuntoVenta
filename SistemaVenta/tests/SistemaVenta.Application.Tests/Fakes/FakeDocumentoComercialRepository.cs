
using SistemaVenta.Application.Interfaces;
using SistemaVenta.Domain.Entities;

namespace SistemaVenta.Application.Tests.Fakes
{
    public class FakeDocumentoComercialRepository
        : IDocumentoComercialRepository
    {
        public DocumentoComercial? Documento { get; set; }

        public decimal TotalAplicado { get; set; }

        public decimal TotalNotasCredito { get; set; }

        public decimal TotalNotasDebito { get; set; }

        // Factura asociada a una venta.
        public Factura? Factura { get; set; }

        // Documento agregado durante una prueba.
        public DocumentoComercial? DocumentoAgregado { get; private set; }

        public List<DocumentoComercial> Notas { get; set; } = new();


        public Task<DocumentoComercial?> ObtenerPorIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            if (Documento?.Id == id)
                return Task.FromResult<DocumentoComercial?>(Documento);

            return Task.FromResult<DocumentoComercial?>(null);
        }

        public Task<decimal> ObtenerTotalAplicadoAsync(
            int documentoComercialId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(TotalAplicado);
        }

        public Task<Factura?> ObtenerFacturaPorVentaIdAsync(
            int ventaId,
            CancellationToken cancellationToken = default)
        {
            if (Factura?.VentaId == ventaId)
                return Task.FromResult<Factura?>(Factura);

            return Task.FromResult<Factura?>(null);
        }

        public Task AgregarAsync(
            DocumentoComercial documento,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(documento);

            DocumentoAgregado = documento;

            return Task.CompletedTask;
        }

        public Task<decimal> ObtenerTotalNotasCreditoAsync(
            int facturaId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(TotalNotasCredito);
        }

        public Task<decimal> ObtenerTotalNotasDebitoAsync(
            int facturaId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(TotalNotasDebito);
        }

        public Task<List<DocumentoComercial>> ObtenerNotasPorFacturaIdAsync(
            int facturaId,
            CancellationToken cancellationToken = default)
        {
            var notas = Notas
                .Where(d =>
                    d is NotaCredito nc && nc.FacturaId == facturaId ||
                    d is NotaDebito nd && nd.FacturaId == facturaId)
                .OrderByDescending(d => d.FechaEmision)
                .ToList();

            return Task.FromResult(notas);
        }


    }
}
