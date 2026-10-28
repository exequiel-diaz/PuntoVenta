
using SistemaVenta.Application.Interfaces;
using SistemaVenta.Domain.Entities;
using System.Reflection;

namespace SistemaVenta.Application.Tests.Fakes
{
    public class FakeVentaRepository : IVentaRepository
    {
        private readonly List<Venta> _ventas = new();

        public IReadOnlyList<Venta> Ventas => _ventas.AsReadOnly();

        public Task<Venta?> ObtenerPorIdAsync(int id)
        {
            var venta = _ventas.FirstOrDefault(v => v.Id == id);

            return Task.FromResult(venta);
        }

        public Task<IReadOnlyList<Venta>> ObtenerTodosAsync()
        {
            IReadOnlyList<Venta> resultado = _ventas.AsReadOnly();

            return Task.FromResult(resultado);
        }

        public Task AgregarAsync(Venta venta)
        {
            ArgumentNullException.ThrowIfNull(venta);

            if (venta.Id == 0)
            {
                var propiedadId = typeof(Venta).GetProperty(
                    nameof(Venta.Id),
                    BindingFlags.Public | BindingFlags.Instance);

                propiedadId!.SetValue(
                    venta,
                    _ventas.Count + 1);
            }

            _ventas.Add(venta);

            return Task.CompletedTask;
        }


    }
}
