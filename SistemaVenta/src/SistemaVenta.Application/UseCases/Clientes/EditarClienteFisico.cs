
using SistemaVenta.Application.Interfaces;
using SistemaVenta.Domain.Entities;
using SistemaVenta.Domain.ValueObjects;

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

            // Normalizamos antes de consultar duplicados.
            var dniNormalizado =
                NormalizadorDocumento.NormalizarDni(dni);

            var cuitNormalizado =
                string.IsNullOrWhiteSpace(cuit)
                    ? null
                    : NormalizadorDocumento.NormalizarCuit(cuit);

            var emailNormalizado = email.Trim();

            if (await _clienteRepository.ExisteDniAsync(
                dniNormalizado, id))
            {
                throw new InvalidOperationException(
                    "Ya existe otro cliente con ese DNI.");
            }

            if (cuitNormalizado is not null &&
                await _clienteRepository.ExisteCuitAsync(
                    cuitNormalizado, id))
            {
                throw new InvalidOperationException(
                    "Ya existe otro cliente con ese CUIT.");
            }

            if (await _clienteRepository.ExisteEmailAsync(
                emailNormalizado, id))
            {
                throw new InvalidOperationException(
                    "Ya existe otro cliente con ese correo electrónico.");
            }

            Cliente.ValidarDatosContacto(
                telefono,
                email,
                direccion);

            fisico.ActualizarDatosPersonales(
                nombre,
                apellido,
                dniNormalizado,
                cuitNormalizado,
                fechaNacimiento);

            fisico.ActualizarDatosContacto(
                telefono,
                emailNormalizado,
                direccion,
                observaciones);

            await _unitOfWork.GuardarCambiosAsync();
        }
    }
}
