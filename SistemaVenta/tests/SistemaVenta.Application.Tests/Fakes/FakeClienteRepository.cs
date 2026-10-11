using SistemaVenta.Application.Interfaces;
using SistemaVenta.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaVenta.Application.Tests.Fakes
{
    public class FakeClienteRepository : IClienteRepository
    {
        private readonly Dictionary<int, Cliente> _clientes = new();

        public void ConfigurarCliente(int id, Cliente cliente)
        {
            _clientes[id] = cliente;
        }

        public Task<Cliente?> ObtenerPorIdAsync(int id)
        {
            _clientes.TryGetValue(id, out var cliente);
            return Task.FromResult(cliente);
        }

        public Task<IReadOnlyList<Cliente>> ObtenerTodosAsync()
        {
            IReadOnlyList<Cliente> clientes =
                _clientes.Values.ToList();

            return Task.FromResult(clientes);
        }


        public Task<bool> ExisteDniAsync(
            string dni,
            int? excluirId = null)
        {
            bool existe = _clientes.Any(par =>
                (!excluirId.HasValue || par.Key != excluirId.Value) &&
                par.Value is ClienteFisico fisico &&
                fisico.Dni == dni);

            return Task.FromResult(existe);
        }

        public Task<bool> ExisteCuitAsync(
            string cuit,
            int? excluirId = null)
        {
            bool existe = _clientes.Any(par =>
                (!excluirId.HasValue || par.Key != excluirId.Value) &&
                (
                    (par.Value is ClienteFisico fisico && fisico.Cuit == cuit) ||
                    (par.Value is ClienteEmpresa empresa && empresa.Cuit == cuit)
                ));

            return Task.FromResult(existe);
        }

        public Task<bool> ExisteEmailAsync(
            string email,
            int? excluirId = null)
        {
            bool existe = _clientes.Any(par =>
                (!excluirId.HasValue || par.Key != excluirId.Value) &&
                string.Equals(
                    par.Value.Email,
                    email,
                    StringComparison.OrdinalIgnoreCase));

            return Task.FromResult(existe);
        }

        public Task AgregarAsync(Cliente cliente)
        {
            int id = _clientes.Count == 0
                ? 1
                : _clientes.Keys.Max() + 1;

            _clientes[id] = cliente;

            return Task.CompletedTask;
        }
    }
}
