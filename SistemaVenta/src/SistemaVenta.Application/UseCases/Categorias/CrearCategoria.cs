using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SistemaVenta.Application.Interfaces;
using SistemaVenta.Domain.Entities;

namespace SistemaVenta.Application.UseCases.Categorias
{
    public class CrearCategoria
    {
        private readonly ICategoriaRepository _categoriaRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CrearCategoria(
            ICategoriaRepository categoriaRepository,
            IUnitOfWork unitOfWork)
        {
            _categoriaRepository = categoriaRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task EjecutarAsync(
            string nombre,
            string? descripcion = null)
        {
            var categoria = new Categoria(nombre, descripcion);

            await _categoriaRepository.AgregarAsync(categoria);

            await _unitOfWork.GuardarCambiosAsync();
        }
    }
}
