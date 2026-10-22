using SistemaVenta.Application.Interfaces;
using SistemaVenta.Domain.Entities;

namespace SistemaVenta.Application.UseCases.CuentasCorrientes;

public class AbrirCuentaCorriente
{
    private readonly ICuentaCorrienteRepository _cuentas;
    private readonly IClienteRepository _clientes;
    private readonly IUnitOfWork _unitOfWork;

    public AbrirCuentaCorriente(
        ICuentaCorrienteRepository cuentas,
        IClienteRepository clientes,
        IUnitOfWork unitOfWork)
    {
        _cuentas = cuentas;
        _clientes = clientes;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> EjecutarAsync(
        int clienteId,
        decimal limiteDeuda,
        string? observaciones = null)
    {
        var cliente = await _clientes.ObtenerPorIdAsync(clienteId);

        if (cliente is null)
            throw new InvalidOperationException(
                "El cliente no existe.");

        if (!cliente.EstaActivo())
            throw new InvalidOperationException(
                "No se puede abrir una cuenta a un cliente inactivo.");

        if (await _cuentas.ExistePorClienteIdAsync(clienteId))
            throw new InvalidOperationException(
                "El cliente ya tiene una cuenta corriente.");

        var cuenta = new CuentaCorriente(
            clienteId,
            limiteDeuda,
            observaciones);

        await _cuentas.AgregarAsync(cuenta);

        await _unitOfWork.GuardarCambiosAsync();

        return cuenta.Id;
    }
}