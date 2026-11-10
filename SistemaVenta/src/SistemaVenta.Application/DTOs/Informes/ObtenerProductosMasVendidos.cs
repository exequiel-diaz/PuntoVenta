using SistemaVenta.Application.DTOs.Informes;
using SistemaVenta.Application.Interfaces;

namespace SistemaVenta.Application.UseCases.Informes
{
    public class ObtenerProductosMasVendidos
    {
        private readonly IInformeVentasRepository _informeVentasRepository;

        public ObtenerProductosMasVendidos(
            IInformeVentasRepository informeVentasRepository)
        {
            _informeVentasRepository = informeVentasRepository;
        }

        public async Task<List<ProductoMasVendidoDto>> EjecutarAsync(
            DateTime fechaDesde,
            DateTime fechaHasta,
            int cantidad = 10,
            string criterio = "cantidad")
        {
            if (fechaDesde > fechaHasta)
            {
                throw new ArgumentException(
                    "La fecha inicial no puede ser posterior a la fecha final.");
            }

            if (cantidad <= 0)
            {
                throw new ArgumentException(
                    "La cantidad de productos debe ser mayor que cero.");
            }

            if (criterio != "cantidad" && criterio != "facturacion")
            {
                throw new ArgumentException(
                    "Criterio de ordenamiento no válido.");
            }

            return await _informeVentasRepository
                .ObtenerProductosMasVendidosAsync(
                    fechaDesde,
                    fechaHasta,
                    cantidad,
                    criterio);
        }
    }
}