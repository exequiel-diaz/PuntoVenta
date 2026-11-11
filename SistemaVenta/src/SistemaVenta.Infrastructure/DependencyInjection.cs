using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuestPDF.Infrastructure;
using SistemaVenta.Application.Interfaces;
using SistemaVenta.Application.UseCases.CuentasCorrientes;
using SistemaVenta.Application.UseCases.Informes;
using SistemaVenta.Application.UseCases.Usuarios;
using SistemaVenta.Application.UseCases.Ventas;
using SistemaVenta.Infrastructure.Informes;
using SistemaVenta.Infrastructure.Initialization;
using SistemaVenta.Infrastructure.Persistence;
using SistemaVenta.Infrastructure.Persistence.Repositories;
using SistemaVenta.Infrastructure.Repositories;
using SistemaVenta.Infrastructure.Security;
using System.Security.Claims;


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
                IInformeInventarioRepository,
                InformeInventarioRepository>();

            services.AddScoped<
                IGeneradorInformeMovimientosPdf,
                GeneradorInformeMovimientosPdf>();


            services.AddScoped<
                IMovimientoStockRepository,
                MovimientoStockRepository>();

            services.AddScoped<IPagoRepository, PagoRepository>();

            services.AddScoped<
                IDocumentoComercialRepository,
                DocumentoComercialRepository>();

            services.AddScoped<
                ITransaccionAplicacionPago,
                TransaccionAplicacionPago>();

            services.AddScoped<
                ITransaccionPagoVenta,
                TransaccionPagoVenta>();

            services.AddScoped<
                ITransaccionCobroVenta,
                TransaccionCobroVenta>();

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
            services.AddScoped<AplicarPagoVentaCuentaCorriente>();
            services.AddScoped<RegistrarCobroVentaCuentaCorriente>();

            // Módulo de Ventas
            services.AddScoped<IVentaRepository, VentaRepository>();
            services.AddScoped<RegistrarVenta>();
            services.AddScoped<VendedorPruebaInicializador>();


            // Módulo de Informes
            services.AddScoped<
                IInformeVentasRepository,
                InformeVentasRepository>();
            services.AddScoped<
                IGeneradorInformeInventarioPdf,
                GeneradorInformeInventarioPdf>();
            services.AddScoped<
                IInformeRentabilidadRepository,
                InformeRentabilidadRepository>();
            services.AddScoped<
                IConfiguracionCreditoRepository,
                ConfiguracionCreditoRepository>();

            services.AddScoped<
                IPermisoUsuarioRepository,
                PermisoUsuarioRepository>();


            return services;
        }
    }
}
