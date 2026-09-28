using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaVenta.Application.DTOs.Categorias
{
    public class CategoriaListadoDto
    {
        public int Id { get; init; }
        public string Nombre { get; init; } = string.Empty;
    }
}
