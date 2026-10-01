using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaVenta.Application.DTOs.Productos
{
    public class ProductoDetalleDto
    {
        public int Id { get; init; }
        public string CodigoInterno { get; init; } = string.Empty;
        public string Nombre { get; init; } = string.Empty;
        public string? Descripcion { get; init; }

        public decimal PrecioVenta { get; init; }
        public decimal Costo { get; init; }

        public decimal StockActual { get; init; }
        public decimal StockMinimo { get; init; }

        public int CategoriaId { get; init; }
    }
}
