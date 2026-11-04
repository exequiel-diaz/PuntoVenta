
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaVenta.Application.DTOs.Vendedores;
using SistemaVenta.Application.UseCases.Vendedores;
using SistemaVenta.Domain.Enums;

namespace SistemaVenta.Web.Controllers;

[Authorize(Roles = nameof(RolUsuario.Administrador))]
public class VendedoresController : Controller
{
    private readonly ObtenerVendedores _obtenerVendedores;
    private readonly CrearVendedor _crearVendedor;
    private readonly EditarVendedor _editarVendedor;
    private readonly CambiarEstadoVendedor _cambiarEstadoVendedor;
    private readonly EliminarVendedor _eliminarVendedor;

    public VendedoresController(
        ObtenerVendedores obtenerVendedores,
        CrearVendedor crearVendedor,
        EditarVendedor editarVendedor,
        CambiarEstadoVendedor cambiarEstadoVendedor,
        EliminarVendedor eliminarVendedor)
    {
        _obtenerVendedores = obtenerVendedores;
        _crearVendedor = crearVendedor;
        _editarVendedor = editarVendedor;
        _cambiarEstadoVendedor = cambiarEstadoVendedor;
        _eliminarVendedor = eliminarVendedor;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var vendedores = await _obtenerVendedores.EjecutarAsync();

        return View(vendedores);
    }

    [HttpGet]
    public IActionResult Crear()
    {
        return View(new CrearVendedorDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(CrearVendedorDto datos)
    {
        if (!ModelState.IsValid)
            return View(datos);

        try
        {
            await _crearVendedor.EjecutarAsync(datos);

            TempData["MensajeExito"] =
                "El vendedor fue registrado correctamente.";

            return RedirectToAction(nameof(Index));
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }

        return View(datos);
    }


    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        var vendedores = await _obtenerVendedores.EjecutarAsync();

        var vendedor = vendedores.FirstOrDefault(v => v.Id == id);

        if (vendedor is null)
            return NotFound();

        var modelo = new EditarVendedorDto
        {
            Id = vendedor.Id,
            NombreUsuario = vendedor.NombreUsuario,
            NombreCompleto = vendedor.NombreCompleto
        };

        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(
        int id,
        EditarVendedorDto datos)
    {
        if (id != datos.Id)
            return BadRequest();

        if (!ModelState.IsValid)
            return View(datos);

        try
        {
            await _editarVendedor.EjecutarAsync(datos);

            TempData["MensajeExito"] =
                "El vendedor fue actualizado correctamente.";

            return RedirectToAction(nameof(Index));
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }

        return View(datos);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstado(
        int id,
        bool activar)
    {
        try
        {
            await _cambiarEstadoVendedor.EjecutarAsync(
                id,
                activar);

            TempData["MensajeExito"] = activar
                ? "El vendedor fue activado correctamente."
                : "El vendedor fue desactivado correctamente.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["MensajeError"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        try
        {
            await _eliminarVendedor.EjecutarAsync(id);

            TempData["MensajeExito"] =
                "El vendedor fue eliminado correctamente.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["MensajeError"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }


}
