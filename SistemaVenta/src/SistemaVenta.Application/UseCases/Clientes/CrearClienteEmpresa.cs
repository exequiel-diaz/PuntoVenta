using SistemaVenta.Application.Interfaces;
using SistemaVenta.Domain.Entities;

namespace SistemaVenta.Application.UseCases.Clientes
{
    public class CrearClienteEmpresa
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CrearClienteEmpresa(
            IClienteRepository clienteRepository,
            IUnitOfWork unitOfWork)
        {
            _clienteRepository = clienteRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task EjecutarAsync(
            string razonSocial,
            string cuit,
            string telefono,
            string email,
            string direccion,
            string? contacto = null,
            string? observaciones = null)
        {
            var cliente = new ClienteEmpresa(
                razonSocial,
                cuit,
                telefono,
                email,
                direccion,
                contacto,
                observaciones);

            if (await _clienteRepository.ExisteCuitAsync(cliente.Cuit))
                throw new InvalidOperationException(
                    "Ya existe un cliente con ese CUIT.");

            if (await _clienteRepository.ExisteEmailAsync(cliente.Email))
                throw new InvalidOperationException(
                    "Ya existe un cliente con ese correo electrónico.");

            await _clienteRepository.AgregarAsync(cliente);

            await _unitOfWork.GuardarCambiosAsync();
        }
    }
}
