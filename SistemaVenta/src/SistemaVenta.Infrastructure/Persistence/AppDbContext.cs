using Microsoft.EntityFrameworkCore;
using SistemaVenta.Application.Interfaces;
using SistemaVenta.Domain.Entities;
using SistemaVenta.Application.Exceptions;

namespace SistemaVenta.Infrastructure.Persistence
{
    public class AppDbContext: DbContext, IUnitOfWork
    {
        public AppDbContext(
           DbContextOptions<AppDbContext> options)
           : base(options)
        {
        }

        public DbSet<Producto> Productos => Set<Producto>();

        public DbSet<Categoria> Categorias => Set<Categoria>();

        public DbSet<MovimientoStock> MovimientosStock =>
            Set<MovimientoStock>();

        public DbSet<Cliente> Clientes => Set<Cliente>();

        public DbSet<CuentaCorriente> CuentasCorrientes
            => Set<CuentaCorriente>();

        public DbSet<MovimientoCuentaCorriente> MovimientosCuentaCorriente
            => Set<MovimientoCuentaCorriente>();

        public DbSet<Venta> Ventas => Set<Venta>();

        public DbSet<DetalleVenta> DetallesVenta => Set<DetalleVenta>();

        public DbSet<Usuario> Usuarios => Set<Usuario>();

        public DbSet<Pago> Pagos => Set<Pago>();

        public DbSet<AplicacionPago> AplicacionesPago =>
            Set<AplicacionPago>();

        public DbSet<AplicacionPagoVenta> AplicacionesPagoVenta =>
            Set<AplicacionPagoVenta>();

        public DbSet<DocumentoComercial> DocumentosComerciales =>
            Set<DocumentoComercial>();


        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(AppDbContext).Assembly);
        }

        public async Task GuardarCambiosAsync()
        {
            try
            {
                await SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new ConflictoConcurrenciaException(
                    "El stock o los datos de un producto cambiaron durante la operación. Revisá la disponibilidad e intentá nuevamente.",
                    ex);
            }
        }


    }
}
