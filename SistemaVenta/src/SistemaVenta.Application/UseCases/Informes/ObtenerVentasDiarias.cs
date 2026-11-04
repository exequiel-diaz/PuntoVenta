
using SistemaVenta.Application.DTOs.Informes;
using SistemaVenta.Application.Interfaces;

namespace SistemaVenta.Application.UseCases.Informes
{
    public class ObtenerVentasDiarias
    {
        private readonly IInformeVentasRepository _informeVentasRepository;

        public ObtenerVentasDiarias(
            IInformeVentasRepository informeVentasRepository)
        {
            _informeVentasRepository = informeVentasRepository;
        }



        public async Task<List<VentaDiariaDto>> EjecutarAsync(
            DateTime fechaDesde,
            DateTime fechaHasta)
        {
            if (fechaDesde > fechaHasta)
            {
                throw new ArgumentException(
                    "La fecha inicial no puede ser posterior a la fecha final.");
            }

            var zonaArgentina = TimeZoneInfo.FindSystemTimeZoneById(
                "America/Argentina/Buenos_Aires");

            var desdeLocal = TimeZoneInfo.ConvertTimeFromUtc(
                fechaDesde,
                zonaArgentina).Date;

            var hastaLocal = TimeZoneInfo.ConvertTimeFromUtc(
                fechaHasta,
                zonaArgentina).Date;

            var ventas = await _informeVentasRepository
                .ObtenerVentasDiariasAsync(fechaDesde, fechaHasta);

            var ventasPorFecha = ventas.ToDictionary(
                v => v.Fecha.Date,
                v => v);

            var resultado = new List<VentaDiariaDto>();

            for (var fecha = desdeLocal;
                 fecha <= hastaLocal;
                 fecha = fecha.AddDays(1))
            {
                if (ventasPorFecha.TryGetValue(fecha, out var venta))
                {
                    resultado.Add(venta);
                }
                else
                {
                    resultado.Add(new VentaDiariaDto
                    {
                        Fecha = fecha,
                        CantidadVentas = 0,
                        TotalVendido = 0
                    });
                }
            }

            return resultado;
        }



    }
}
