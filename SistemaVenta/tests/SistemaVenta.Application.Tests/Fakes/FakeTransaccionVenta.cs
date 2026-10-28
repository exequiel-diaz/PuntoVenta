
using SistemaVenta.Application.Interfaces;

namespace SistemaVenta.Application.Tests.Fakes
{
    public class FakeTransaccionVenta : ITransaccionVenta
    {
        public async Task EjecutarAsync(
            int ventaId,
            Func<Task> operacion,
            CancellationToken cancellationToken = default)
        {
            await operacion();
        }
    }
}
