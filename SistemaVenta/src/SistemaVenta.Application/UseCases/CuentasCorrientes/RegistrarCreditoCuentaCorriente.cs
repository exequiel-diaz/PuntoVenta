
using SistemaVenta.Application.Interfaces;

namespace SistemaVenta.Application.UseCases.CuentasCorrientes;

public class RegistrarCreditoCuentaCorriente
{
    private readonly ICuentaCorrienteRepository _cuentas;
    private readonly IUnitOfWork _unitOfWork;

    public RegistrarCreditoCuentaCorriente(
        ICuentaCorrienteRepository cuentas,
        IUnitOfWork unitOfWork)
    {
        _cuentas = cuentas;
        _unitOfWork = unitOfWork;
    }

    public async Task EjecutarAsync(
        int cuentaId,
        decimal importe,
        string descripcion)
    {
        var cuenta = await _cuentas.ObtenerPorIdAsync(cuentaId);

        if (cuenta is null)
            throw new InvalidOperationException(
                "La cuenta corriente no existe.");

        cuenta.RegistrarCredito(importe, descripcion);

        await _unitOfWork.GuardarCambiosAsync();
    }
}
