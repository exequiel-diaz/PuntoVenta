
namespace SistemaVenta.Application.DTOs.Categorias
{
    public class CategoriaListadoDto
    {
        public int Id { get; init; }

        public string Nombre { get; init; } = string.Empty;

        public string? Descripcion { get; init; }

        public bool Activa { get; init; }
    }
}
