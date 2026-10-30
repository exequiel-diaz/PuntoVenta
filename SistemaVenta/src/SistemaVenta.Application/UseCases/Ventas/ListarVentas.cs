
using SistemaVenta.Application.DTOs.Ventas;
using SistemaVenta.Application.Interfaces;
using SistemaVenta.Application.UseCases.Clientes;

namespace SistemaVenta.Application.UseCases.Ventas
{
    public class ListarVentas
    {
        private readonly IVentaRepository _ventaRepository;
        private readonly ListarClientes _listarClientes;

        public ListarVentas(
            IVentaRepository ventaRepository,
            ListarClientes listarClientes)
        {
            _ventaRepository = ventaRepository;
            _listarClientes = listarClientes;
        }

        public async Task<IReadOnlyList<VentaListadoDto>> EjecutarAsync()
        {
            var ventas = await _ventaRepository.ObtenerTodosAsync();

            var clientes = await _listarClientes.EjecutarAsync();

            var nombresClientes = clientes.ToDictionary(
                c => c.Id,
                c => c.Nombre);

            return ventas
                .Select(v => new VentaListadoDto
                {
                    Id = v.Id,
                    ClienteId = v.ClienteId,

                    NombreCliente = !v.ClienteId.HasValue
                        ? "Consumidor final"
                        : nombresClientes.TryGetValue(
                            v.ClienteId.Value,
                            out var nombre)
                            ? nombre
                            : $"Cliente #{v.ClienteId.Value}",

                    Fecha = v.Fecha,
                    Estado = v.Estado,
                    Total = v.Total
                })
                .ToList();
        }
    }
}
