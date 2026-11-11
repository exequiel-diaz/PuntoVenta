
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using SistemaVenta.Application.Interfaces;
using SistemaVenta.Domain.Enums;

namespace SistemaVenta.Web.Security;

public sealed class PermisoAuthorizationHandler
    : AuthorizationHandler<PermisoRequirement>
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IPermisoUsuarioRepository _permisos;

    public PermisoAuthorizationHandler(
        IUsuarioRepository usuarios,
        IPermisoUsuarioRepository permisos)
    {
        _usuarios = usuarios;
        _permisos = permisos;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermisoRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true)
            return;

        var identificador = context.User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(identificador, out var usuarioId))
            return;

        var usuario = await _usuarios.ObtenerPorIdAsync(
            usuarioId);

        if (usuario is null || !usuario.Activo)
            return;

        // Administrador: acceso completo.
        if (usuario.Rol == RolUsuario.Administrador)
        {
            context.Succeed(requirement);
            return;
        }

        // Solo los vendedores pueden recibir
        // permisos individuales.
        if (usuario.Rol != RolUsuario.Vendedor)
            return;

        var autorizado = await _permisos.TienePermisoAsync(
            usuarioId,
            requirement.CodigoPermiso);

        if (autorizado)
            context.Succeed(requirement);
    }
}
