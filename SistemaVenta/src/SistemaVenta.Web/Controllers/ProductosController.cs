using Microsoft.AspNetCore.Mvc;
using SistemaVenta.Application.UseCases.Productos;

namespace SistemaVenta.Web.Controllers
{
    public class ProductosController : Controller
    {
        private readonly ListarProductos _listarProductos;

        public ProductosController(ListarProductos listarProductos)
        {
            _listarProductos = listarProductos;
        }
        public async Task<IActionResult> Index()
        {
            var productos = await _listarProductos.EjecutarAsync();

            return View(productos);
        }
    }
}
