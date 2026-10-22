using SistemaVenta.Application.Interfaces;

namespace SistemaVenta.Application.UseCases.CuentasCorrientes;

public class ActivarCuentaCorriente
{
    private readonly ICuentaCorrienteRepository _cuentas;
    private readonly IUnitOfWork _unitOfWork;

    public ActivarCuentaCorriente(
        ICuentaCorrienteRepository cuentas,
        IUnitOfWork unitOfWork)
    {
        _cuentas = cuentas;
        _unitOfWork = unitOfWork;
    }

    public async Task EjecutarAsync(int cuentaId)
    {
        var cuenta = await _cuentas.ObtenerPorIdAsync(cuentaId);

        if (cuenta is null)
            throw new InvalidOperationException(
                "La cuenta corriente no existe.");

        cuenta.Activar();

        await _unitOfWork.GuardarCambiosAsync();
    }
}