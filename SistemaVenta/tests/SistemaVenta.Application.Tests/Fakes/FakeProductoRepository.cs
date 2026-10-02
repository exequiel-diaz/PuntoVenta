using SistemaVenta.Domain.Entities;
using SistemaVenta.Application.Interfaces;

namespace SistemaVenta.Application.Tests.Fakes
{
    public class FakeProductoRepository: IProductoRepository
    {
        private readonly Dictionary<int, Producto> _productos = new();
        public void ConfigurarProducto(
            int id,
            Producto producto)
        {
            _productos[id] = producto;
        }
       
        public Task<Producto?> ObtenerPorIdAsync(int id)
        {
            _productos.TryGetValue(id, out var producto);
            return Task.FromResult(producto);
        }

        public Task<IReadOnlyList<Producto>> ObtenerTodosAsync()
        {
            IReadOnlyList<Producto> productos= 
                _productos.Values.ToList();
            
            return Task.FromResult(productos);
        }

        public Task AgregarAsync(Producto producto)
        {
            var id = _productos.Count + 1;
            _productos[id] = producto;

            return Task.CompletedTask;
        }
        
    }
}
