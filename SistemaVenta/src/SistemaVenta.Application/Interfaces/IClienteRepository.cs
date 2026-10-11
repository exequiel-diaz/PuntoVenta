using SistemaVenta.Domain.Entities;

namespace SistemaVenta.Application.Interfaces
{
    public interface IClienteRepository
    {
        Task<Cliente?> ObtenerPorIdAsync(int id);
        Task<IReadOnlyList<Cliente>> ObtenerTodosAsync();
        Task<bool> ExisteDniAsync(string dni, int? excluirId = null);
        Task<bool> ExisteCuitAsync(string cuit, int? excluirId = null);
        Task<bool> ExisteEmailAsync(string email, int? excluirId = null);
        Task AgregarAsync(Cliente cliente);
    }
}
