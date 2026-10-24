
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SistemaVenta.Application.DTOs.Ventas;
using SistemaVenta.Application.Exceptions;
using SistemaVenta.Application.UseCases.Clientes;
using SistemaVenta.Application.UseCases.Productos;
using SistemaVenta.Application.UseCases.Ventas;
using SistemaVenta.Domain.Enums;
using SistemaVenta.Web.ViewModels.Ventas;

namespace SistemaVenta.Web.Controllers;

public class VentasController : Controller
{
    private readonly ListarClientes _listarClientes;
    private readonly ListarProductos _listarProductos;
    private readonly RegistrarVenta _registrarVenta;
    private readonly ListarVentas _listarVentas;
    private readonly ObtenerDetalleVenta _obtenerDetalleVenta;
    private readonly AnularVenta _anularVenta;

    public VentasController(
        ListarClientes listarClientes,
        ListarProductos listarProductos,
        RegistrarVenta registrarVenta,
        ListarVentas listarVentas,
        ObtenerDetalleVenta obtenerDetalleVenta,
        AnularVenta anularVenta)
    {
        _listarClientes = listarClientes;
        _listarProductos = listarProductos;
        _registrarVenta = registrarVenta;
        _listarVentas = listarVentas;
        _obtenerDetalleVenta = obtenerDetalleVenta;
        _anularVenta = anularVenta;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var ventas = await _listarVentas.EjecutarAsync();

        return View(ventas);
    }

    [HttpGet]
    public async Task<IActionResult> Crear()
    {
        var modelo = new RegistrarVentaViewModel
        {
            Detalles = new List<DetalleVentaViewModel>
            {
                new()
                {
                    Cantidad = 1m
                }
            }
        };

        await CargarOpcionesAsync(modelo);

        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(
        RegistrarVentaViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            await CargarOpcionesAsync(modelo);
            return View(modelo);
        }

        try
        {
            var solicitud = new RegistrarVentaDto
            {
                ClienteId = modelo.ClienteId,
                Detalles = modelo.Detalles
                    .Select(d => new DetalleVentaEntradaDto
                    {
                        ProductoId = d.ProductoId,
                        Cantidad = d.Cantidad
                    })
                    .ToList()
            };

            await _registrarVenta.EjecutarAsync(solicitud);

            TempData["MensajeExito"] =
                "La venta se registró correctamente.";

            return RedirectToAction(nameof(Crear));
        }
        catch (ConflictoConcurrenciaException)
        {
            ModelState.AddModelError(
                string.Empty,
                "El stock cambió mientras se registraba la venta. " +
                "Revisá los productos e intentá nuevamente.");
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(
                string.Empty,
                ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(
                string.Empty,
                ex.Message);
        }

        await CargarOpcionesAsync(modelo);

        return View(modelo);
    }

    private async Task CargarOpcionesAsync(
        RegistrarVentaViewModel modelo)
    {
        var clientes = await _listarClientes.EjecutarAsync();

        modelo.ClientesDisponibles = clientes
            .Where(c => c.Estado == EstadoCliente.Activo)
            .Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = $"{c.Nombre} - {c.Documento}"
            })
            .ToList();

        var productos = await _listarProductos.EjecutarAsync();

        modelo.ProductosDisponibles = productos
            .Where(p =>
                p.Estado == EstadoProducto.Activo.ToString() &&
                p.StockActual > 0)
            .Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = $"{p.Nombre} - " +
                       $"${p.PrecioVenta:N2} " +
                       $"(Stock: {p.StockActual:N3})"
            })
            .ToList();

        modelo.PreciosProductos = productos
            .Where(p =>
                p.Estado == EstadoProducto.Activo.ToString() &&
                p.StockActual > 0)
            .ToDictionary(
                p => p.Id,
                p => p.PrecioVenta);
    }


    [HttpGet]
    public async Task<IActionResult> Detalle(int id)
    {
        var venta = await _obtenerDetalleVenta.EjecutarAsync(id);

        if (venta is null)
        {
            return NotFound();
        }

        return View(venta);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Anular(int id)
    {
        try
        {
            await _anularVenta.EjecutarAsync(id);

            TempData["MensajeExito"] =
                $"La venta #{id} se anuló correctamente.";
        }
        catch (ConflictoConcurrenciaException)
        {
            TempData["MensajeError"] =
                "La venta fue modificada por otro usuario. " +
                "Actualizá la información e intentá nuevamente.";
        }
        catch (ArgumentException ex)
        {
            TempData["MensajeError"] = ex.Message;
        }
        catch (InvalidOperationException ex)
        {
            TempData["MensajeError"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

}
