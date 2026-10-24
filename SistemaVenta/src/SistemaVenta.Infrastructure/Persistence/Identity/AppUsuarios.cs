using Microsoft.AspNetCore.Identity;
using SistemaVenta.Domain.Entities;


namespace SistemaVenta.Infrastructure.Persistence.Identity
{
    public class AppUsuarios : IdentityUser
    {
        public bool Estado { get; set; } = true;

        public int EmpleadoId { get; set; }
        public Empleado Empleado { get; set; } = null!;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public DateTime? UltimoAcceso { get; set; }
    }

}  