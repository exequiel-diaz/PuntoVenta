using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SistemaVenta.Application.Interfaces;
using SistemaVenta.Application.UseCases.CuentasCorrientes;
using SistemaVenta.Infrastructure.Persistence;
using SistemaVenta.Infrastructure.Persistence.Repositories;
using SistemaVenta.Infrastructure.Repositories;
using SistemaVenta.Application.UseCases.Ventas;


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

            return services;
        }
    }
}
