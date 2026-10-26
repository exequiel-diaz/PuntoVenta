using SistemaVenta.Application;
using SistemaVenta.Application.UseCases.CuentasCorrientes;
using SistemaVenta.Infrastructure;
using SistemaVenta.Infrastructure.Initialization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using SistemaVenta.Infrastructure.Security;


var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddAuthentication(
        CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "SistemaVenta.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy =
            CookieSecurePolicy.Always;
        options.Cookie.SameSite =
            SameSiteMode.Lax;

        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccesoDenegado";

        options.ExpireTimeSpan =
            TimeSpan.FromMinutes(30);

        options.SlidingExpiration = true;

        options.Events.OnValidatePrincipal =
            ValidarSesionCookie.ValidarAsync;
    });

builder.Services.AddAuthorization();


// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    var politica = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    options.Filters.Add(new AuthorizeFilter(politica));
});

//agregados por mi
builder.Services.AddApplication();
builder.Services.AddInfrastructure(
    builder.Configuration);

builder.Services.AddScoped<RegistrarDebitoCuentaCorriente>();
builder.Services.AddScoped<RegistrarCreditoCuentaCorriente>();



builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode =
        StatusCodes.Status429TooManyRequests;

    options.AddPolicy("login", context =>
    {
        var direccionIp =
            context.Connection.RemoteIpAddress?.ToString()
            ?? "desconocida";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: direccionIp,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            });
    });
});



var app = builder.Build();

if (args.Contains("--inicializar-admin"))
{
    if (!app.Environment.IsDevelopment())
    {
        throw new InvalidOperationException(
            "La inicialización del administrador solo está permitida en Development.");
    }

    using var scope = app.Services.CreateScope();

    var inicializador = scope.ServiceProvider
        .GetRequiredService<AdministradorInicializador>();

    await inicializador.InicializarAsync();

    Console.WriteLine(
        "Administrador inicial creado correctamente.");

    return;
}


if (args.Contains("--inicializar-vendedor-prueba"))
{
    if (!app.Environment.IsDevelopment())
    {
        throw new InvalidOperationException(
            "La creación del vendedor de prueba solo está permitida en Development.");
    }

    using var scope = app.Services.CreateScope();

    var inicializador = scope.ServiceProvider
        .GetRequiredService<VendedorPruebaInicializador>();

    await inicializador.InicializarAsync();

    Console.WriteLine(
        "Vendedor de prueba creado correctamente.");

    return;
}


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();

public partial class Program { }
