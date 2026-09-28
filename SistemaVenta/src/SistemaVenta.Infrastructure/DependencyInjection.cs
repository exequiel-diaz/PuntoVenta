using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SistemaVenta.Application.Interfaces;
using SistemaVenta.Infrastructure.Persistence;
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
                configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(connectionString));

            services.AddScoped<IProductoRepository, ProductoRepository>();

            services.AddScoped<
                IMovimientoStockRepository,
                MovimientoStockRepository>();

            services.AddScoped<IUnitOfWork>(
                provider =>
                    provider.GetRequiredService<AppDbContext>());

            return services;
        }
    }
}
