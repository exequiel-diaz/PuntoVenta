
using SistemaVenta.Application.DTOs.Informes;

namespace SistemaVenta.Application.Interfaces;

public interface IGeneradorInformeInventarioPdf
{
    byte[] Generar(
        IReadOnlyList<InventarioProductoDto> productos);
}
