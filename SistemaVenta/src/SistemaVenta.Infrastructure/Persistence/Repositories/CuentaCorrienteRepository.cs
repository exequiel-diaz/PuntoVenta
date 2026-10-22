using Microsoft.EntityFrameworkCore;
using SistemaVenta.Application.Interfaces;
using SistemaVenta.Domain.Entities;
using SistemaVenta.Infrastructure.Persistence;

namespace SistemaVenta.Infrastructure.Repositories;

public class CuentaCorrienteRepository : ICuentaCorrienteRepository
{
    private readonly AppDbContext _context;

    public CuentaCorrienteRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<CuentaCorriente?> ObtenerPorIdAsync(int id)
    {
        return await _context.CuentasCorrientes
            .Include(c => c.Movimientos)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<CuentaCorriente?> ObtenerPorClienteIdAsync(int clienteId)
    {
        return await _context.CuentasCorrientes
            .Include(c => c.Movimientos)
            .FirstOrDefaultAsync(c => c.ClienteId == clienteId);
    }

    public async Task<bool> ExistePorClienteIdAsync(int clienteId)
    {
        return await _context.CuentasCorrientes
            .AnyAsync(c => c.ClienteId == clienteId);
    }

    public async Task AgregarAsync(CuentaCorriente cuenta)
    {
        await _context.CuentasCorrientes.AddAsync(cuenta);
    }
}