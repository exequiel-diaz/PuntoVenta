using SistemaVenta.Application.Interfaces;
using SistemaVenta.Domain.Entities;
using System.Collections.Generic;

namespace SistemaVenta.Application.Tests.Fakes
{
    public class FakeCategoriaRepository
        : ICategoriaRepository
    {
        private readonly Dictionary<int, Categoria> _categorias = new();
        public void ConfigurarCategoria(
            int id,
            Categoria categoria)
        {
            _categorias[id] = categoria;
        }

        public Task<Categoria?> ObtenerPorIdAsync(int id)
        {
            _categorias.TryGetValue(id, out var categoria);
            return Task.FromResult(categoria);
        }

        public Task<IReadOnlyList<Categoria>> ObtenerTodasAsync()
        {
            IReadOnlyList<Categoria> categorias =
                _categorias.Values.ToList();

            return Task.FromResult(categorias);
        }

        public Task AgregarAsync(Categoria categoria)
        {
            var id = _categorias.Count + 1;
            _categorias[id] = categoria;

            return Task.CompletedTask;
        }

        public Task EliminarAsync(Categoria categoria)
        {
            // Reemplazar _categorias por el nombre de la colección
            // que ya utiliza tu FakeCategoriaRepository.
            _categorias.Remove(categoria.Id);

            return Task.CompletedTask;
        }

        public Task<bool> TieneProductosAsync(int categoriaId)
        {
            // Implementación provisional para pruebas que
            // no necesitan simular productos asociados.
            return Task.FromResult(false);
        }

    }
}
