using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SistemaVenta.Application.Interfaces;
using SistemaVenta.Domain.Entities;
using SistemaVenta.Infrastructure.Persistence.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace SistemaVenta.Infrastructure.Persistence
{
    public class AppDbContext: IdentityDbContext<AppUsuarios,IdentityRole,string>, IUnitOfWork
    {
        public AppDbContext(
           DbContextOptions<AppDbContext> options)
           : base(options)
        {
        }

        public DbSet<Persona> Personas => Set<Persona>();
        public DbSet<Producto> Productos => Set<Producto>();
    
        public DbSet<Empleado> Empleados { get; set; }
        public DbSet<Categoria> Categorias => Set<Categoria>();

        public DbSet<MovimientoStock> MovimientosStock =>
            Set<MovimientoStock>();

        protected override void OnModelCreating(
      ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(AppDbContext).Assembly);

            modelBuilder.Entity<AppUsuarios>()
                .HasOne(u => u.Empleado)
                .WithOne()
                .HasForeignKey<AppUsuarios>(u => u.EmpleadoId);

            modelBuilder.Entity<Empleado>()
                .HasOne(e => e.Persona)
                .WithOne()
                .HasForeignKey<Empleado>(e => e.PersonaId);

            modelBuilder.Entity<Persona>()
                .Property(p => p.FechaNacimiento)
                .HasColumnType("date");

            modelBuilder.Entity<Empleado>()
                .Property(e => e.FechaIngreso)
                .HasColumnType("date");
        }
        public async Task GuardarCambiosAsync()
        {
            await SaveChangesAsync();
        }

    }
}
