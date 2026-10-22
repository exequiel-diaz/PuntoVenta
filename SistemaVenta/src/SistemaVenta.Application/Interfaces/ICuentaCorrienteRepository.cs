using SistemaVenta.Domain.Entities;

namespace SistemaVenta.Application.Interfaces;

public interface ICuentaCorrienteRepository
{
    Task<CuentaCorriente?> ObtenerPorIdAsync(int id);

    Task<CuentaCorriente?> ObtenerPorClienteIdAsync(int clienteId);

    Task<bool> ExistePorClienteIdAsync(int clienteId);

    Task AgregarAsync(CuentaCorriente cuenta);
}