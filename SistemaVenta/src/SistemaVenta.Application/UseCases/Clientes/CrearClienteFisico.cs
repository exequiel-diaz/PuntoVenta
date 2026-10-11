using SistemaVenta.Application.Interfaces;
using SistemaVenta.Domain.Entities;

namespace SistemaVenta.Application.UseCases.Clientes
{
    public class CrearClienteFisico
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CrearClienteFisico(
            IClienteRepository clienteRepository,
            IUnitOfWork unitOfWork)
        {
            _clienteRepository = clienteRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task EjecutarAsync(
            string nombre,
            string apellido,
            string dni,
            string telefono,
            string email,
            string direccion,
            string? cuit = null,
            DateTime? fechaNacimiento = null,
            string? observaciones = null)
        {
            var cliente = new ClienteFisico(
                nombre,
                apellido,
                dni,
                telefono,
                email,
                direccion,
                cuit,
                fechaNacimiento,
                observaciones);

            if (await _clienteRepository.ExisteDniAsync(cliente.Dni))
                throw new InvalidOperationException(
                    "Ya existe un cliente con ese DNI.");

            if (cliente.Cuit is not null &&
                await _clienteRepository.ExisteCuitAsync(cliente.Cuit))
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
