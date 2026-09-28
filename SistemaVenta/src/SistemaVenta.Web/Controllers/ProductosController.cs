using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SistemaVenta.Application.UseCases.Productos;
using SistemaVenta.Application.UseCases.Categorias;
using SistemaVenta.Web.Models.Productos;

namespace SistemaVenta.Web.Controllers
{
    public class ProductosController : Controller
    {
        private readonly ListarProductos _listarProductos;
        private readonly CrearProducto _crearProducto;
        private readonly ListarCategorias _listarCategorias;
        public ProductosController(
            ListarProductos listarProductos,
            CrearProducto crearProducto,
            ListarCategorias listarCategorias)
        {
            _listarProductos = listarProductos;
            _crearProducto = crearProducto;
            _listarCategorias = listarCategorias;
        }
        public async Task<IActionResult> Index()
        {
            var productos = await _listarProductos.EjecutarAsync();

            return View(productos);
        }

        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            var model = new CrearProductoViewModel();

            await CargarCategoriasAsync(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(
            CrearProductoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await CargarCategoriasAsync(model);

                return View(model);
            }

            await _crearProducto.EjecutarAsync(
                model.CodigoInterno,
                model.Nombre,
                model.Descripcion,
                model.PrecioVenta,
                model.Costo,
                model.StockMinimo,
                model.CategoriaId);

            return RedirectToAction(nameof(Index));
        }

        private async Task CargarCategoriasAsync(
            CrearProductoViewModel model)
        {
            var categorias =
                await _listarCategorias.EjecutarAsync();

            model.Categorias = categorias.Select(c =>
                new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Nombre
                });
        }
    }
}
