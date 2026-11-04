
using SistemaVenta.Application.DTOs.Informes;
using SistemaVenta.Application.Interfaces;

namespace SistemaVenta.Application.UseCases.Informes
{
    public class ObtenerInformeVentasPorPeriodo
    {
        private readonly IInformeVentasRepository _informeVentasRepository;

        public ObtenerInformeVentasPorPeriodo(
            IInformeVentasRepository informeVentasRepository)
        {
            _informeVentasRepository = informeVentasRepository;
        }

        public async Task<InformeVentasPorPeriodoDto> EjecutarAsync(
            DateTime fechaDesde,
            DateTime fechaHasta)
        {
            if (fechaDesde > fechaHasta)
            {
                throw new ArgumentException(
                    "La fecha inicial no puede ser posterior a la fecha final.");
            }

            return await _informeVentasRepository
                .ObtenerVentasPorPeriodoAsync(fechaDesde, fechaHasta);
        }
    }
}
