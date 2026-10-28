
using SistemaVenta.Domain.Entities;

namespace SistemaVenta.Application.Interfaces
{
    public interface IDocumentoComercialRepository
    {
        Task<DocumentoComercial?> ObtenerPorIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<decimal> ObtenerTotalAplicadoAsync(
            int documentoComercialId,
            CancellationToken cancellationToken = default);

        Task<Factura?> ObtenerFacturaPorVentaIdAsync(
            int ventaId,
            CancellationToken cancellationToken = default);

        Task AgregarAsync(
            DocumentoComercial documento,
            CancellationToken cancellationToken = default);
    }
}
