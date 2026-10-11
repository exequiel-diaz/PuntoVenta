using SistemaVenta.Application.DTOs.Clientes;
using SistemaVenta.Application.Interfaces;
using SistemaVenta.Domain.Entities;

namespace SistemaVenta.Application.UseCases.Clientes
{
    public class ListarClientes
    {
        private readonly IClienteRepository _clienteRepository;

        public ListarClientes(IClienteRepository clienteRepository)
        {
            _clienteRepository = clienteRepository;
        }

        public async Task<IReadOnlyList<ClienteListadoDto>> EjecutarAsync()
        {
            var clientes = await _clienteRepository.ObtenerTodosAsync();

            return clientes.Select(cliente =>
            {
                string tipo;
                string nombre;
                string documento;

                switch (cliente)
                {
                    case ClienteFisico fisico:
                        tipo = "Físico";
                        nombre = $"{fisico.Nombre} {fisico.Apellido}";
                        documento = fisico.Dni;
                        break;

                    case ClienteEmpresa empresa:
                        tipo = "Empresa";
                        nombre = empresa.RazonSocial;
                        documento = empresa.Cuit;
                        break;

                    default:
                        throw new InvalidOperationException(
                            "Tipo de cliente no reconocido.");
                }

                return new ClienteListadoDto
                {
                    Id = cliente.Id,
                    Tipo = tipo,
                    Nombre = nombre,
                    Documento = documento,
                    Email = cliente.Email,
                    Telefono = cliente.Telefono,
                    Estado = cliente.Estado
                };
            }).ToList();
        }
    }
}
