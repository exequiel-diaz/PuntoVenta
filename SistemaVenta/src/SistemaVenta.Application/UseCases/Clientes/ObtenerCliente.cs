using SistemaVenta.Application.DTOs.Clientes;
using SistemaVenta.Application.Interfaces;
using SistemaVenta.Domain.Entities;

namespace SistemaVenta.Application.UseCases.Clientes
{
    public class ObtenerCliente
    {
        private readonly IClienteRepository _clienteRepository;

        public ObtenerCliente(IClienteRepository clienteRepository)
        {
            _clienteRepository = clienteRepository;
        }

        public async Task<ClienteDetalleDto> EjecutarAsync(int id)
        {
            var cliente = await _clienteRepository.ObtenerPorIdAsync(id);

            if (cliente is null)
                throw new InvalidOperationException(
                    "El cliente no existe.");

            var dto = new ClienteDetalleDto
            {
                Id = cliente.Id,
                Telefono = cliente.Telefono,
                Email = cliente.Email,
                Direccion = cliente.Direccion,
                Observaciones = cliente.Observaciones,
                Estado = cliente.Estado
            };

            switch (cliente)
            {
                case ClienteFisico fisico:
                    dto.Tipo = "Físico";
                    dto.Nombre = fisico.Nombre;
                    dto.Apellido = fisico.Apellido;
                    dto.Dni = fisico.Dni;
                    dto.Cuit = fisico.Cuit;
                    dto.FechaNacimiento = fisico.FechaNacimiento;
                    break;

                case ClienteEmpresa empresa:
                    dto.Tipo = "Empresa";
                    dto.RazonSocial = empresa.RazonSocial;
                    dto.Cuit = empresa.Cuit;
                    dto.Contacto = empresa.Contacto;
                    break;

                default:
                    throw new InvalidOperationException(
                        "Tipo de cliente no reconocido.");
            }

            return dto;
        }
    }
}
