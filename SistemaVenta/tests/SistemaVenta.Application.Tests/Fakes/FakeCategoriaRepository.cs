using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SistemaVenta.Application.Interfaces;
using SistemaVenta.Domain.Entities;

namespace SistemaVenta.Application.Tests.Fakes
{
    public class FakeCategoriaRepository
        : ICategoriaRepository
    {
        public Categoria? Categoria { get; set; }

        public List<Categoria> Categorias { get; } = new();

        public Task<Categoria?> ObtenerPorIdAsync(int id)
        {
            return Task.FromResult(Categoria);
        }

        public Task<IReadOnlyList<Categoria>> ObtenerTodasAsync()
        {
            return Task.FromResult<IReadOnlyList<Categoria>>(
                Categorias);
        }

        public Task AgregarAsync(Categoria categoria)
        {
            Categorias.Add(categoria);

            return Task.CompletedTask;
        }
    }
}
