using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SistemaVenta.Application.Interfaces;
using SistemaVenta.Infrastructure.Persistence;
using SistemaVenta.Infrastructure.Persistence.Identity;
using SistemaVenta.Infrastructure.Persistence.Repositories;

namespace SistemaVenta.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var connectionString =
                configuration.GetConnectionString("ConexionPostgreSQL");

            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(connectionString));

            // Configuración de Identity

            services.AddIdentity<AppUsuarios, IdentityRole>()
                    .AddEntityFrameworkStores<AppDbContext>()
                    .AddDefaultTokenProviders();
            


            services.AddScoped<IProductoRepository, ProductoRepository>();

            services.AddScoped<
                IMovimientoStockRepository,
                MovimientoStockRepository>();

            services.AddScoped<ICategoriaRepository, CategoriaRepository>();

            services.AddScoped<IUnitOfWork>(
                provider =>
                    provider.GetRequiredService<AppDbContext>());

            return services;
        }
    }
}
