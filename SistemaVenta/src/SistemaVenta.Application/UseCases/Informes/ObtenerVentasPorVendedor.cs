
using SistemaVenta.Application.DTOs.Informes;
using SistemaVenta.Application.Interfaces;

namespace SistemaVenta.Application.UseCases.Informes
{
    public class ObtenerVentasPorVendedor
    {
        private readonly IInformeVentasRepository _repository;

        public ObtenerVentasPorVendedor(
            IInformeVentasRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<VentaPorVendedorDto>> EjecutarAsync(
            DateTime fechaDesde,
            DateTime fechaHasta)
        {
            if (fechaDesde > fechaHasta)
            {
                throw new ArgumentException(
                    "La fecha inicial no puede ser posterior a la fecha final.");
            }

            return await _repository.ObtenerVentasPorVendedorAsync(
                fechaDesde,
                fechaHasta);
        }
    }
}
