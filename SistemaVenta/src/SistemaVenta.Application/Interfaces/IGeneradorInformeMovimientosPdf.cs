
using SistemaVenta.Application.DTOs.Informes;

namespace SistemaVenta.Application.Interfaces;

public interface IGeneradorInformeMovimientosPdf
{
    byte[] Generar(
        IReadOnlyList<MovimientoStockInformeDto> movimientos,
        DateTime fechaDesde,
        DateTime fechaHasta);
}
