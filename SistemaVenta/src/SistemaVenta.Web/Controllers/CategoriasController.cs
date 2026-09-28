using Microsoft.AspNetCore.Mvc;
using SistemaVenta.Application.UseCases.Categorias;
using SistemaVenta.Web.Models.Categorias;

namespace SistemaVenta.Web.Controllers
{
    public class CategoriasController : Controller
    {
        private readonly ListarCategorias _listarCategorias;
        private readonly CrearCategoria _crearCategoria;

        public CategoriasController(
            ListarCategorias listarCategorias,
            CrearCategoria crearCategoria)
        {
            _listarCategorias = listarCategorias;
            _crearCategoria = crearCategoria;
        }

        public async Task<IActionResult> Index()
        {
            var categorias = await _listarCategorias.EjecutarAsync();

            return View(categorias);
        }

        [HttpGet]
        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(
    CrearCategoriaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            await _crearCategoria.EjecutarAsync(
                model.Nombre,
                model.Descripcion);

            return RedirectToAction(nameof(Index));
        }

    }
}
