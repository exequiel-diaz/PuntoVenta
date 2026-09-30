using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SistemaVenta.Application.UseCases.Productos;
using SistemaVenta.Application.UseCases.Categorias;
using SistemaVenta.Web.Models.Productos;
using SistemaVenta.Application.UseCases.Stock;

namespace SistemaVenta.Web.Controllers
{
    public class ProductosController : Controller
    {
        private readonly ListarProductos _listarProductos;
        private readonly CrearProducto _crearProducto;
        private readonly ListarCategorias _listarCategorias;
        private readonly ObtenerProducto _obtenerProducto;
        private readonly RegistrarIngresoStock _registrarIngresoStock;
        public ProductosController(
            ListarProductos listarProductos,
            CrearProducto crearProducto,
            ListarCategorias listarCategorias,
            ObtenerProducto obtenerProducto,
            RegistrarIngresoStock registrarIngresoStock)
        {
            _listarProductos = listarProductos;
            _crearProducto = crearProducto;
            _listarCategorias = listarCategorias;
            _obtenerProducto = obtenerProducto;
            _registrarIngresoStock = registrarIngresoStock;
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

        [HttpGet]
        public async Task<IActionResult> IngresarStock(int id)
        {
            var producto = await _obtenerProducto.EjecutarAsync(id);

            if (producto is null)
                return NotFound();

            var model = new IngresarStockViewModel
            {
                ProductoId = producto.Id,
                NombreProducto = producto.Nombre
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IngresarStock(
        IngresarStockViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var producto =
                    await _obtenerProducto.EjecutarAsync(model.ProductoId);

                if (producto is null)
                    return NotFound();

                model.NombreProducto = producto.Nombre;

                return View(model);
            }

            await _registrarIngresoStock.EjecutarAsync(
                model.ProductoId,
                model.Cantidad,
                model.Motivo);

            return RedirectToAction(nameof(Index));
        }

    }
}
