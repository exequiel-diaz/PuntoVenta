using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SistemaVenta.Application.Interfaces;
using SistemaVenta.Domain.Entities;

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

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(AppDbContext).Assembly);
        }
        public async Task GuardarCambiosAsync()
        {
            await SaveChangesAsync();
        }

    }
}
