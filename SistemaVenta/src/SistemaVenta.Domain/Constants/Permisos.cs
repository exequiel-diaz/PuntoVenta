
namespace SistemaVenta.Domain.Constants;

public static class Permisos
{
    public const string CrearProductos =
        "Productos.Crear";

    public const string EditarProductos =
        "Productos.Editar";

    public const string IngresarStock =
        "Stock.Ingresar";

    public const string AbrirCuentaCorriente =
        "CuentasCorrientes.Abrir";

    public const string RegistrarCobros =
        "CuentasCorrientes.RegistrarCobros";

    public static readonly IReadOnlySet<string>
        Delegables = new HashSet<string>
        {
            CrearProductos,
            EditarProductos,
            IngresarStock,
            AbrirCuentaCorriente,
            RegistrarCobros
        };
}
