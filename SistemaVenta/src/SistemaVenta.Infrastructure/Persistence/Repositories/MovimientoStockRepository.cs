using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SistemaVenta.Application.Interfaces;
using SistemaVenta.Domain.Entities;

namespace SistemaVenta.Infrastructure.Persistence.Repositories
{
    public class MovimientoStockRepository 
        : IMovimientoStockRepository
    {
        private readonly AppDbContext _context;

        public MovimientoStockRepository(
            AppDbContext context)
        {
            _context = context;
        }

        public async Task AgregarAsync(
            MovimientoStock movimiento)
        {
            await _context.MovimientosStock
                .AddAsync(movimiento);
        }
    }
}
