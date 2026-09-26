using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SistemaVenta.Application.Interfaces;
using SistemaVenta.Domain.Entities;

namespace SistemaVenta.Application.Tests.Fakes
{
    public class FakeMovimientoStockRepository: IMovimientoStockRepository
    {
        public MovimientoStock? MovimientoAgregado { get; private set; }

        public Task AgregarAsync(MovimientoStock movimiento)
        {
            MovimientoAgregado = movimiento;

            return Task.CompletedTask;
        }
    }
}
