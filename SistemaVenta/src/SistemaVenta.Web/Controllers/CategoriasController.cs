
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaVenta.Application.UseCases.Categorias;
using SistemaVenta.Web.Models.Categorias;

namespace SistemaVenta.Web.Controllers
{
    [Authorize(Roles ="Administrador")]
    public class CategoriasController : Controller
    {
        private readonly ListarCategorias _listarCategorias;
        private readonly CrearCategoria _crearCategoria;
        private readonly ObtenerCategoria _obtenerCategoria;
        private readonly EditarCategoria _editarCategoria;
        private readonly ActivarCategoria _activarCategoria;
        private readonly DesactivarCategoria _desactivarCategoria;
        private readonly EliminarCategoria _eliminarCategoria;

        public CategoriasController(
            ListarCategorias listarCategorias,
            CrearCategoria crearCategoria,
            ObtenerCategoria obtenerCategoria,
            EditarCategoria editarCategoria,
            ActivarCategoria activarCategoria,
            DesactivarCategoria desactivarCategoria,
            EliminarCategoria eliminarCategoria)
        {
            _listarCategorias = listarCategorias;
            _crearCategoria = crearCategoria;
            _obtenerCategoria = obtenerCategoria;
            _editarCategoria = editarCategoria;
            _activarCategoria = activarCategoria;
            _desactivarCategoria = desactivarCategoria;
            _eliminarCategoria = eliminarCategoria;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var categorias =
                await _listarCategorias.EjecutarAsync();

            return View(categorias);
        }

        [HttpGet]
        public IActionResult Crear()
        {
            return View(new CrearCategoriaViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(
            CrearCategoriaViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                await _crearCategoria.EjecutarAsync(
                    model.Nombre,
                    model.Descripcion);

                TempData["Mensaje"] =
                    "Categoría creada correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(
                    nameof(model.Nombre), ex.Message);

                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var categoria =
                await _obtenerCategoria.EjecutarAsync(id);

            if (categoria is null)
                return NotFound();

            var model = new EditarCategoriaViewModel
            {
                Id = categoria.Id,
                Nombre = categoria.Nombre,
                Descripcion = categoria.Descripcion
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(
            int id,
            EditarCategoriaViewModel model)
        {
            if (id != model.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                await _editarCategoria.EjecutarAsync(
                    id,
                    model.Nombre,
                    model.Descripcion);

                TempData["Mensaje"] =
                    "Categoría actualizada correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(
                    nameof(model.Nombre), ex.Message);

                return View(model);
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activar(int id)
        {
            try
            {
                await _activarCategoria.EjecutarAsync(id);

                TempData["Mensaje"] =
                    "Categoría activada correctamente.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Desactivar(int id)
        {
            try
            {
                await _desactivarCategoria.EjecutarAsync(id);

                TempData["Mensaje"] =
                    "Categoría desactivada correctamente.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            try
            {
                await _eliminarCategoria.EjecutarAsync(id);

                TempData["Mensaje"] =
                    "Categoría eliminada correctamente.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
