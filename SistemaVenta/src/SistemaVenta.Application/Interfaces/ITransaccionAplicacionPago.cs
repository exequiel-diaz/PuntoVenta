
namespace SistemaVenta.Application.Interfaces
{
    public interface ITransaccionAplicacionPago
    {
        Task EjecutarAsync(
            int documentoComercialId,
            int pagoId,
            Func<Task> operacion,
            CancellationToken cancellationToken = default);
    }
}
