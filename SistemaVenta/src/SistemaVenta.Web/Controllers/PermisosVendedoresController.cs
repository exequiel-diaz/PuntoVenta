
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaVenta.Application.Interfaces;
using SistemaVenta.Application.UseCases.Usuarios;
using SistemaVenta.Domain.Constants;
using SistemaVenta.Domain.Enums;
using SistemaVenta.Web.Models.Vendedores;

namespace SistemaVenta.Web.Controllers;

[Authorize(Roles = nameof(RolUsuario.Administrador))]
[Route("Vendedores/Permisos")]
public class PermisosVendedoresController : Controller
{
    private readonly IUsuarioRepository _usuarios;
    private readonly ObtenerPermisosUsuario _obtenerPermisos;
    private readonly ActualizarPermisosUsuario _actualizarPermisos;

    public PermisosVendedoresController(
        IUsuarioRepository usuarios,
        ObtenerPermisosUsuario obtenerPermisos,
        ActualizarPermisosUsuario actualizarPermisos)
    {
        _usuarios = usuarios;
        _obtenerPermisos = obtenerPermisos;
        _actualizarPermisos = actualizarPermisos;
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Editar(int id)
    {
        var usuario = await _usuarios.ObtenerPorIdAsync(id);

        if (usuario is null ||
            usuario.Rol != RolUsuario.Vendedor)
        {
            return NotFound();
        }

        var permisos = await _obtenerPermisos.EjecutarAsync(id);

        var modelo = new PermisosVendedorViewModel
        {
            UsuarioId = id,
            NombreCompleto = usuario.NombreCompleto,
            PermisosSeleccionados = permisos
        };

        return View(modelo);
    }

    [HttpPost("{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(
        int id,
        PermisosVendedorViewModel modelo)
    {
        if (id != modelo.UsuarioId)
            return BadRequest();

        var usuario = await _usuarios.ObtenerPorIdAsync(id);

        if (usuario is null ||
            usuario.Rol != RolUsuario.Vendedor)
        {
            return NotFound();
        }

        modelo.NombreCompleto = usuario.NombreCompleto;

        if (!ModelState.IsValid)
            return View(modelo);

        try
        {
            await _actualizarPermisos.EjecutarAsync(
                id,
                modelo.PermisosSeleccionados
                    ?? new List<string>());

            TempData["MensajeExito"] =
                "Los permisos fueron actualizados correctamente.";

            return RedirectToAction(nameof(Editar), new { id });
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }

        return View(modelo);
    }
}
