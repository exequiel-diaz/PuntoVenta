using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SistemaVenta.Domain.Entities;

namespace SistemaVenta.Application.Interfaces
{
    public interface IProductoRepository
    {
        Task<Producto?> ObtenerPorIdAsync(int id);
        Task<IReadOnlyList<Producto>> ObtenerTodosAsync();
        Task AgregarAsync(Producto producto);
    }
}
