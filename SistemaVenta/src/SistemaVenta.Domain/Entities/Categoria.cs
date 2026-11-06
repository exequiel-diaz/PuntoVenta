
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
            Actualizar(nombre, descripcion);
            Activa = true;
        }

        public void Actualizar(string nombre, string? descripcion = null)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                throw new ArgumentException(
                    "El nombre de la categoría es obligatorio.",
                    nameof(nombre));
            }

            Nombre = nombre.Trim();

            Descripcion = string.IsNullOrWhiteSpace(descripcion)
                ? null
                : descripcion.Trim();
        }

        public void Desactivar()
        {
            if (!Activa)
            {
                throw new InvalidOperationException(
                    "La categoría ya está desactivada.");
            }

            Activa = false;
        }

        public void Activar()
        {
            if (Activa)
            {
                throw new InvalidOperationException(
                    "La categoría ya está activa.");
            }

            Activa = true;
        }
    }
}
