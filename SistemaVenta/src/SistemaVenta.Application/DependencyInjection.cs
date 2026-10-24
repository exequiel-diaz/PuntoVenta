using Microsoft.Extensions.DependencyInjection;
using SistemaVenta.Application.UseCases.Categorias;
using SistemaVenta.Application.UseCases.Clientes;
using SistemaVenta.Application.UseCases.Productos;
using SistemaVenta.Application.UseCases.Stock;
using SistemaVenta.Application.UseCases.Ventas;


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
            services.AddScoped<EditarProducto>();
            services.AddScoped<DesactivarProducto>();
            services.AddScoped<ActivarProducto>();

            services.AddScoped<ListarCategorias>();
            services.AddScoped<CrearCategoria>();

            services.AddScoped<CrearClienteFisico>();
            services.AddScoped<CrearClienteEmpresa>();
            services.AddScoped<ListarClientes>();
            services.AddScoped<ObtenerCliente>();
            services.AddScoped<EditarClienteFisico>();
            services.AddScoped<EditarClienteEmpresa>();
            services.AddScoped<DesactivarCliente>();
            services.AddScoped<ActivarCliente>();

            // Ventas
            services.AddScoped<ListarVentas>();
            services.AddScoped<ObtenerDetalleVenta>();
            services.AddScoped<AnularVenta>();


            return services;
        }

    }
}
