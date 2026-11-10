
using SistemaVenta.Application.DTOs.Informes;
using SistemaVenta.Application.Interfaces;

namespace SistemaVenta.Application.UseCases.Informes
{
    public class ObtenerMejoresClientes
    {
        private readonly IInformeVentasRepository _repository;

        public ObtenerMejoresClientes(
            IInformeVentasRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<ClienteTopDto>> EjecutarAsync(
            DateTime fechaDesde,
            DateTime fechaHasta,
            int cantidad = 10)
        {
            if (fechaDesde > fechaHasta)
            {
                throw new ArgumentException(
                    "La fecha inicial no puede ser posterior a la final.");
            }

            if (cantidad is not (5 or 10 or 20 or 50))
            {
                throw new ArgumentException(
                    "La cantidad debe ser 5, 10, 20 o 50.");
            }

            return await _repository.ObtenerMejoresClientesAsync(
                fechaDesde,
                fechaHasta,
                cantidad);
        }
    }
}
