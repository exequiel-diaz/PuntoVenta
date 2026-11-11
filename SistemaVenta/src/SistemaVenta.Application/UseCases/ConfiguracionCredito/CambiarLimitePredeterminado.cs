
using SistemaVenta.Application.Interfaces;

namespace SistemaVenta.Application.UseCases.ConfiguracionCredito;

public class CambiarLimitePredeterminado
{
    private readonly IConfiguracionCreditoRepository _configuracion;
    private readonly IUnitOfWork _unitOfWork;

    public CambiarLimitePredeterminado(
        IConfiguracionCreditoRepository configuracion,
        IUnitOfWork unitOfWork)
    {
        _configuracion = configuracion;
        _unitOfWork = unitOfWork;
    }

    public async Task EjecutarAsync(decimal nuevoLimite)
    {
        var configuracion = await _configuracion
            .ObtenerConfiguracionAsync();

        if (configuracion is null)
        {
            throw new InvalidOperationException(
                "No se encontró la configuración de crédito.");
        }

        // La entidad valida que el límite sea válido.
        configuracion.CambiarLimite(nuevoLimite);

        await _unitOfWork.GuardarCambiosAsync();
    }
}
