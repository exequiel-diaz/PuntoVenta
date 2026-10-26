
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SistemaVenta.Integration.Tests.Security;

public class TestAuthHandler
    : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "TestScheme";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var rol = Request.Headers["X-Test-Role"].ToString();

        if (string.IsNullOrWhiteSpace(rol))
        {
            return Task.FromResult(
                AuthenticateResult.NoResult());
        }

        if (rol != "Administrador" && rol != "Vendedor")
        {
            return Task.FromResult(
                AuthenticateResult.Fail(
                    "Rol de prueba no válido."));
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "999"),
            new Claim(ClaimTypes.Name, "usuario_prueba"),
            new Claim(ClaimTypes.Role, rol)
        };

        var identidad = new ClaimsIdentity(claims, SchemeName);

        var principal = new ClaimsPrincipal(identidad);

        var ticket = new AuthenticationTicket(
            principal,
            SchemeName);

        return Task.FromResult(
            AuthenticateResult.Success(ticket));
    }
}
