
using SistemaVenta.Application.Interfaces;

namespace SistemaVenta.Application.Tests.Fakes
{
    public class FakeTransaccionAplicacionPago
        : ITransaccionAplicacionPago
    {
        public bool OperacionEjecutada { get; private set; }

        public async Task EjecutarAsync(
            int documentoComercialId,
            Func<Task> operacion,
            CancellationToken cancellationToken = default)
        {
            OperacionEjecutada = true;

            await operacion();
        }
    }
}
