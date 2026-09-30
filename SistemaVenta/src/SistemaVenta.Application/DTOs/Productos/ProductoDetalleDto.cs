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
        public decimal StockActual { get; init; }
    }
}
