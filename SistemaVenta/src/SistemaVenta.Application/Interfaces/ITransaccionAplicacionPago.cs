
namespace SistemaVenta.Application.Interfaces
{
    public interface ITransaccionAplicacionPago
    {
        Task EjecutarAsync(
            int documentoComercialId,
            Func<Task> operacion,
            CancellationToken cancellationToken = default);
    }
}
