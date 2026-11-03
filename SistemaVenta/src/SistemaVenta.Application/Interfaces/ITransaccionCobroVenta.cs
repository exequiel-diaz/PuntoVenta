namespace SistemaVenta.Application.Interfaces;

public interface ITransaccionCobroVenta
{
    Task<T> EjecutarAsync<T>(
        Func<Task<T>> operacion,
        CancellationToken cancellationToken = default);
}