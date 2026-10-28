
namespace SistemaVenta.Application.Interfaces
{
    public interface ITransaccionVenta
    {
        Task EjecutarAsync(
            int ventaId,
            Func<Task> operacion,
            CancellationToken cancellationToken = default);
    }
}
