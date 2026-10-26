
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;
using SistemaVenta.Application.Interfaces;

namespace SistemaVenta.Infrastructure.Security;

public static class ValidarSesionCookie
{
    public static async Task ValidarAsync(
        CookieValidatePrincipalContext context)
    {
        var idTexto = context.Principal?
            .FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(idTexto, out int usuarioId))
        {
            await RechazarSesionAsync(context);
            return;
        }

        var repositorio = context.HttpContext
            .RequestServices
            .GetRequiredService<IUsuarioRepository>();

        var usuario = await repositorio
            .ObtenerPorIdAsync(usuarioId);

        if (usuario is null || !usuario.Activo)
        {
            await RechazarSesionAsync(context);
            return;
        }

        var rolEnCookie = context.Principal?
            .FindFirstValue(ClaimTypes.Role);

        var nombreEnCookie = context.Principal?
            .FindFirstValue(ClaimTypes.Name);

        if (rolEnCookie != usuario.Rol.ToString()
            || nombreEnCookie != usuario.NombreUsuario)
        {
            await RechazarSesionAsync(context);
        }
    }

    private static async Task RechazarSesionAsync(
        CookieValidatePrincipalContext context)
    {
        context.RejectPrincipal();

        await context.HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);
    }
}
