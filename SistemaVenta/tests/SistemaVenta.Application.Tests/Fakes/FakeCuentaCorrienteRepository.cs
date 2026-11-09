using SistemaVenta.Application.Interfaces;
using SistemaVenta.Domain.Entities;

namespace SistemaVenta.Application.Tests.Fakes;

public class FakeCuentaCorrienteRepository : ICuentaCorrienteRepository
{
    private readonly Dictionary<int, CuentaCorriente> _cuentas = new();

    private int _siguienteId = 1;

    public Task<CuentaCorriente?> ObtenerPorIdAsync(int id)
    {
        _cuentas.TryGetValue(id, out var cuenta);

        return Task.FromResult(cuenta);
    }

    public Task<CuentaCorriente?> ObtenerPorClienteIdAsync(int clienteId)
    {
        var cuenta = _cuentas.Values
            .FirstOrDefault(c => c.ClienteId == clienteId);

        return Task.FromResult(cuenta);
    }

    public Task<bool> ExistePorClienteIdAsync(int clienteId)
    {
        var existe = _cuentas.Values
            .Any(c => c.ClienteId == clienteId);

        return Task.FromResult(existe);
    }

    public Task AgregarAsync(CuentaCorriente cuenta)
    {
        // Simulamos la asignación de Id del repositorio.
        // El Id de dominio tiene setter privado, así que no lo
        // modificamos aquí.
        _cuentas.Add(_siguienteId++, cuenta);

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<CuentaCorriente>> ObtenerTodasAsync()
    {
        IReadOnlyList<CuentaCorriente> cuentas = _cuentas.Values.ToList();

        return Task.FromResult(cuentas);
    }

    public void ConfigurarCuenta(int id, CuentaCorriente cuenta)
    {
        _cuentas[id] = cuenta;
    }
}