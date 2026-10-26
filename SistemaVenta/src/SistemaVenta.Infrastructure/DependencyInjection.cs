using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SistemaVenta.Application.Interfaces;
using SistemaVenta.Application.UseCases.CuentasCorrientes;
using SistemaVenta.Infrastructure.Persistence;
using SistemaVenta.Infrastructure.Persistence.Repositories;
using SistemaVenta.Infrastructure.Repositories;
using SistemaVenta.Application.UseCases.Ventas;
using SistemaVenta.Infrastructure.Security;
using SistemaVenta.Application.UseCases.Usuarios;
using SistemaVenta.Infrastructure.Initialization;


namespace SistemaVenta.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var connectionString =
                configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(connectionString));

            services.AddScoped<IProductoRepository, ProductoRepository>();

            services.AddScoped<
                IMovimientoStockRepository,
                MovimientoStockRepository>();

            services.AddScoped<ICategoriaRepository, CategoriaRepository>();

            services.AddScoped<IUnitOfWork>(
                provider =>
                    provider.GetRequiredService<AppDbContext>());

            services.AddScoped<IClienteRepository, ClienteRepository>();
            services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            services.AddScoped<IPasswordHasher, PasswordHasherService>();
            services.AddScoped<RegistrarUsuario>();
            services.AddScoped<AutenticarUsuario>();
            services.AddScoped<AdministradorInicializador>();

            services.AddScoped<
                ICuentaCorrienteRepository,
                CuentaCorrienteRepository>();

            services.AddScoped<AbrirCuentaCorriente>();
            services.AddScoped<ObtenerCuentaCorriente>();
            services.AddScoped<ModificarLimiteDeuda>();
            services.AddScoped<SuspenderCuentaCorriente>();
            services.AddScoped<ActivarCuentaCorriente>();
            services.AddScoped<CerrarCuentaCorriente>();

            // Módulo de Ventas
            services.AddScoped<IVentaRepository, VentaRepository>();
            services.AddScoped<RegistrarVenta>();
            services.AddScoped<VendedorPruebaInicializador>();

            return services;
        }
    }
}
