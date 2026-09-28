using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SistemaVenta.Application.Interfaces;

namespace SistemaVenta.Application.Tests.Fakes
{
    public class FakeUnitOfWork : IUnitOfWork
    {
        public bool GuardarCambiosFueLlamado { get; private set; }

        public Task GuardarCambiosAsync()
        {
            GuardarCambiosFueLlamado = true;

            return Task.CompletedTask;
        }
    }
}
