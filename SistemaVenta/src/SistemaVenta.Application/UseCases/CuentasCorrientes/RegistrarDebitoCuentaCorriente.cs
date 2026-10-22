
using SistemaVenta.Application.Interfaces;

namespace SistemaVenta.Application.UseCases.CuentasCorrientes;

public class RegistrarDebitoCuentaCorriente
{
    private readonly ICuentaCorrienteRepository _cuentas;
    private readonly IUnitOfWork _unitOfWork;

    public RegistrarDebitoCuentaCorriente(
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

        cuenta.RegistrarDebito(importe, descripcion);

        await _unitOfWork.GuardarCambiosAsync();
    }
}
