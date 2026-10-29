namespace SistemaVenta.Application.Interfaces;

public interface ITransaccionPagoVenta
{
    Task EjecutarAsync(
        int ventaId,
        int pagoId,
        Func<Task> operacion,
        CancellationToken cancellationToken = default);
}