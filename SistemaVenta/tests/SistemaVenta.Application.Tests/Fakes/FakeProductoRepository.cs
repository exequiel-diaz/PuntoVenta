using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SistemaVenta.Domain.Entities;
using SistemaVenta.Application.Interfaces;

namespace SistemaVenta.Application.Tests.Fakes
{
    public class FakeProductoRepository: IProductoRepository
    {
        public Producto? Producto { get; set; }
        //public Producto? ProductoActualizado { get; private set; }
        public Task<Producto?> ObtenerPorIdAsync(int id)
        {
            return Task.FromResult(Producto);
        }
        /*
        public Task ActualizarAsync(Producto producto)
        {
            ProductoActualizado = producto;

            return Task.CompletedTask;
        }
        */
    }
}
