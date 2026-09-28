using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaVenta.Domain.Entities
{
    public class Categoria
    {
        public int Id { get; private set; }
        public string Nombre { get; private set; } = string.Empty;
        public string? Descripcion { get; private set; }
        public bool Activa { get; private set; }
        private Categoria()
        {

        }
        public Categoria(string nombre, string? descripcion = null)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                throw new ArgumentException(
                    "El nombre de la categoría es obligatorio.",
                    nameof(nombre));
            }

            Nombre = nombre.Trim();
            Descripcion = descripcion?.Trim();
            Activa = true;
        }
        
    }
}
