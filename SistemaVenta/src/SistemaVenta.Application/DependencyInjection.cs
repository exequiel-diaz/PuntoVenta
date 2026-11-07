using Microsoft.Extensions.DependencyInjection;
using SistemaVenta.Application.UseCases.Categorias;
using SistemaVenta.Application.UseCases.Clientes;
using SistemaVenta.Application.UseCases.Pagos;
using SistemaVenta.Application.UseCases.Productos;
using SistemaVenta.Application.UseCases.Stock;
using SistemaVenta.Application.UseCases.Ventas;
using SistemaVenta.Application.UseCases.Informes;
using SistemaVenta.Application.UseCases.Vendedores;


namespace SistemaVenta.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(
            this IServiceCollection services)
        {
            services.AddScoped<RegistrarIngresoStock>();
            services.AddScoped<RegistrarSalidaStock>();

            services.AddScoped<CorregirInventario>();
            services.AddScoped<ObtenerInformeInventario>();
            services.AddScoped<ObtenerInformeMovimientosStock>();


            services.AddScoped<ListarProductos>();
            services.AddScoped<CrearProducto>();
            services.AddScoped<ObtenerProducto>();
            services.AddScoped<EditarProducto>();
            services.AddScoped<DesactivarProducto>();
            services.AddScoped<ActivarProducto>();
            services.AddScoped<EliminarProducto>();

            services.AddScoped<ListarCategorias>();
            services.AddScoped<CrearCategoria>();
            services.AddScoped<EditarCategoria>();
            services.AddScoped<ActivarCategoria>();
            services.AddScoped<DesactivarCategoria>();
            services.AddScoped<EliminarCategoria>();

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

            services.AddScoped<RegistrarPago>();

            // Módulo de Informes
            services.AddScoped<ObtenerInformeVentasPorPeriodo>();
            services.AddScoped<ObtenerProductosMasVendidos>();
            services.AddScoped<ObtenerVentasDiarias>();
            services.AddScoped<ObtenerVentasPorVendedor>();

            // módl de vendedore
            services.AddScoped<ObtenerVendedores>();
            services.AddScoped<CrearVendedor>();
            services.AddScoped<EditarVendedor>();
            services.AddScoped<CambiarEstadoVendedor>();
            services.AddScoped<EliminarVendedor>();

            return services;
        }

    }
}
