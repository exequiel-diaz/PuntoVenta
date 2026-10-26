
namespace SistemaVenta.Web.Security;

public static class Roles
{
    public const string Administrador = "Administrador";
    public const string Vendedor = "Vendedor";

    public const string AdministradorOVendedor =
        Administrador + "," + Vendedor;
}
