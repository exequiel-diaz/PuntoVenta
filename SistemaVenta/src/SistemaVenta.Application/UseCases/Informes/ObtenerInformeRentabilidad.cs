
using SistemaVenta.Application.DTOs.Informes;
using SistemaVenta.Application.Interfaces;

namespace SistemaVenta.Application.UseCases.Informes;

public class ObtenerInformeRentabilidad
{
    private readonly IInformeRentabilidadRepository _repository;

    public ObtenerInformeRentabilidad(
        IInformeRentabilidadRepository repository)
    {
        _repository = repository;
    }

    public Task<InformeRentabilidadDto> EjecutarAsync(
        DateTime fechaDesde,
        DateTime fechaHasta,
        int top = 10)
    {
        if (fechaDesde > fechaHasta)
            throw new ArgumentException(
                "La fecha inicial no puede superar la final.");

        if (top is not (5 or 10 or 20 or 50))
            throw new ArgumentException(
                "El Top debe ser 5, 10, 20 o 50.");

        return _repository.ObtenerAsync(
            fechaDesde, fechaHasta, top);
    }
}
