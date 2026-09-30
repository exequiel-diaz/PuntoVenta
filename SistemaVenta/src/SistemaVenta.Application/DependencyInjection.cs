using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SistemaVenta.Application.UseCases.Stock;
using SistemaVenta.Application.UseCases.Productos;
using SistemaVenta.Application.UseCases.Categorias;

namespace SistemaVenta.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(
            this IServiceCollection services)
        {
            services.AddScoped<RegistrarIngresoStock>();

            services.AddScoped<ListarProductos>();
            services.AddScoped<CrearProducto>();
            services.AddScoped<ObtenerProducto>();

            services.AddScoped<ListarCategorias>();
            services.AddScoped<CrearCategoria>();
            

            return services;
        }

    }
}
