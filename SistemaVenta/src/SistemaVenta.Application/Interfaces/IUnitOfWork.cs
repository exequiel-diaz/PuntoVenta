using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaVenta.Application.Interfaces
{
    public interface IUnitOfWork
    {
        Task GuardarCambiosAsync();//confirma los cambios realizados durante una caso de uso
    }
}
