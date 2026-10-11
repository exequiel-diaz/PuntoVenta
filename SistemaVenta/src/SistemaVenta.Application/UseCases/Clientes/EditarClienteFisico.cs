
using SistemaVenta.Application.Interfaces;
using SistemaVenta.Domain.Entities;

namespace SistemaVenta.Application.UseCases.Clientes
{
    public class EditarClienteFisico
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly IUnitOfWork _unitOfWork;

        public EditarClienteFisico(
            IClienteRepository clienteRepository,
            IUnitOfWork unitOfWork)
        {
            _clienteRepository = clienteRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task EjecutarAsync(
            int id,
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
            var cliente = await _clienteRepository.ObtenerPorIdAsync(id);

            if (cliente is not ClienteFisico fisico)
                throw new InvalidOperationException(
                    "El cliente físico no existe.");

            // Validamos los campos obligatorios antes de consultar duplicados.
            if (string.IsNullOrWhiteSpace(nombre) ||
                string.IsNullOrWhiteSpace(apellido) ||
                string.IsNullOrWhiteSpace(dni) ||
                string.IsNullOrWhiteSpace(telefono) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(direccion))
            {
                throw new ArgumentException(
                    "Los campos obligatorios no pueden estar vacíos.");
            }

            dni = dni.Trim();
            email = email.Trim();

            cuit = string.IsNullOrWhiteSpace(cuit)
                ? null
                : cuit.Trim();

            if (await _clienteRepository.ExisteDniAsync(dni, id))
                throw new InvalidOperationException(
                    "Ya existe otro cliente con ese DNI.");

            if (cuit is not null &&
                await _clienteRepository.ExisteCuitAsync(cuit, id))
                throw new InvalidOperationException(
                    "Ya existe otro cliente con ese CUIT.");

            if (await _clienteRepository.ExisteEmailAsync(email, id))
                throw new InvalidOperationException(
                    "Ya existe otro cliente con ese correo electrónico.");

            fisico.ActualizarDatosPersonales(
                nombre,
                apellido,
                dni,
                cuit,
                fechaNacimiento);

            fisico.ActualizarDatosContacto(
                telefono,
                email,
                direccion,
                observaciones);

            await _unitOfWork.GuardarCambiosAsync();
        }
    }
}
