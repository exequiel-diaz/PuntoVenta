using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SistemaVenta.Domain.Enums;

namespace SistemaVenta.Domain.Entities
{
    public class Producto
    {
        public int Id { get; private set; }
        public string CodigoInterno { get; private set; } = string.Empty;
        public string Nombre { get; private set; } = string.Empty;
        public string? Descripcion { get; private set; }
        public decimal PrecioVenta {  get; private set; }
        public decimal Costo { get; private set; }
        public decimal StockActual {  get; private set; }
        public decimal StockMinimo {  get; private set; }
        public EstadoProducto Estado {  get; private set; }
        public DateTime FechaAlta {  get; private set; }


        public Producto(
            string codigoInterno,
            string nombre,
            decimal precioVenta,
            decimal costo,
            decimal stockMinimo)
        {
            if (string.IsNullOrWhiteSpace(codigoInterno))
            {
                throw new ArgumentException(
                    "El código interno es obligatorio.",
                    nameof(codigoInterno));
            }

            if (string.IsNullOrWhiteSpace(nombre))
            {
                throw new ArgumentException(
                    "El nombre es obligatorio.",
                    nameof(nombre));
            }

            if (precioVenta < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(precioVenta),
                    "El precio de venta no puede ser negativo.");
            }

            if (costo < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(costo),
                    "El costo no puede ser negativo.");
            }

            if (stockMinimo < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(stockMinimo),
                    "El stock mínimo no puede ser negativo.");
            }

            CodigoInterno = codigoInterno.Trim();
            Nombre = nombre.Trim();
            PrecioVenta = precioVenta;
            Costo = costo;
            StockMinimo = stockMinimo;

            StockActual = 0;
            Estado = EstadoProducto.Activo;
            FechaAlta = DateTime.UtcNow;
        }
    }

}


