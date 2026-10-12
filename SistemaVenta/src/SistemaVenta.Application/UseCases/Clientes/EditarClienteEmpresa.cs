
using SistemaVenta.Application.Interfaces;
using SistemaVenta.Domain.Entities;
using SistemaVenta.Domain.ValueObjects;

namespace SistemaVenta.Application.UseCases.Clientes
{
    public class EditarClienteEmpresa
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly IUnitOfWork _unitOfWork;

        public EditarClienteEmpresa(
            IClienteRepository clienteRepository,
            IUnitOfWork unitOfWork)
        {
            _clienteRepository = clienteRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task EjecutarAsync(
            int id,
            string razonSocial,
            string cuit,
            string telefono,
            string email,
            string direccion,
            string? contacto = null,
            string? observaciones = null)
        {
            var cliente =
                await _clienteRepository.ObtenerPorIdAsync(id);

            if (cliente is not ClienteEmpresa empresa)
                throw new InvalidOperationException(
                    "La empresa no existe.");

            if (string.IsNullOrWhiteSpace(razonSocial) ||
                string.IsNullOrWhiteSpace(cuit) ||
                string.IsNullOrWhiteSpace(telefono) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(direccion))
            {
                throw new ArgumentException(
                    "Los campos obligatorios no pueden estar vacíos.");
            }

            // Normalizamos antes de consultar duplicados.
            var cuitNormalizado =
                NormalizadorDocumento.NormalizarCuit(cuit);

            var emailNormalizado = email.Trim();

            if (await _clienteRepository.ExisteCuitAsync(
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

            empresa.ActualizarDatosEmpresa(
                razonSocial,
                cuitNormalizado,
                contacto);

            empresa.ActualizarDatosContacto(
                telefono,
                emailNormalizado,
                direccion,
                observaciones);

            await _unitOfWork.GuardarCambiosAsync();
        }
    }
}
