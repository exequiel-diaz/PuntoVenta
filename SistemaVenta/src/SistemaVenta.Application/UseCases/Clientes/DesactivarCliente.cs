
using SistemaVenta.Application.Interfaces;

namespace SistemaVenta.Application.UseCases.Clientes
{
    public class DesactivarCliente
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DesactivarCliente(
            IClienteRepository clienteRepository,
            IUnitOfWork unitOfWork)
        {
            _clienteRepository = clienteRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task EjecutarAsync(int id)
        {
            var cliente = await _clienteRepository.ObtenerPorIdAsync(id);

            if (cliente is null)
                throw new InvalidOperationException(
                    "El cliente no existe.");

            cliente.Desactivar();

            await _unitOfWork.GuardarCambiosAsync();
        }
    }
}
