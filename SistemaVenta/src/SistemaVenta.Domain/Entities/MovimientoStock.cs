using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SistemaVenta.Domain.Enums;

namespace SistemaVenta.Domain.Entities
{
    public class MovimientoStock
    {
        public int Id { get; private set; }
        public DateTime Fecha { get; private set; }
        public TipoMovimientoStock Tipo { get; private set; }
        public decimal Cantidad { get; private set; }
        public decimal StockAnterior { get; private set; }
        public decimal StockPosterior { get; private set; }
        public string? Motivo { get; private set; }
        public Producto Producto { get; private set; }

        private MovimientoStock()
        {
            Producto = null!;
        }
        public MovimientoStock(
           Producto producto,
           TipoMovimientoStock tipo,
           decimal cantidad,
           decimal stockAnterior,
           decimal stockPosterior,
           string? motivo = null)
        {
            if (producto is null)
            {
                throw new ArgumentNullException(
                    nameof(producto),
                    "El producto es obligatorio.");
            }

            if (cantidad <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(cantidad),
                    "La cantidad debe ser mayor que cero.");
            }

            if (stockAnterior < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(stockAnterior),
                    "El stock anterior no puede ser negativo.");
            }

            if (stockPosterior < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(stockPosterior),
                    "El stock posterior no puede ser negativo.");
            }

            Producto = producto;
            Tipo = tipo;
            Cantidad = cantidad;
            StockAnterior = stockAnterior;
            StockPosterior = stockPosterior;
            Motivo = motivo?.Trim();

            Fecha = DateTime.UtcNow;
        }
    }
}
