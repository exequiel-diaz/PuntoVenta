
using SistemaVenta.Domain.Enums;

namespace SistemaVenta.Domain.Entities;

public class Usuario
{
    public int Id { get; private set; }
    public string NombreUsuario { get; private set; } = string.Empty;
    public string NombreCompleto { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public RolUsuario Rol { get; private set; }
    public bool Activo { get; private set; }

    private Usuario() { }

    public Usuario(
        string nombreUsuario,
        string nombreCompleto,
        string passwordHash,
        RolUsuario rol)
    {
        if (string.IsNullOrWhiteSpace(nombreUsuario))
            throw new ArgumentException(
                "El nombre de usuario es obligatorio.");

        if (string.IsNullOrWhiteSpace(nombreCompleto))
            throw new ArgumentException(
                "El nombre completo es obligatorio.");

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException(
                "El hash de la contraseña es obligatorio.");

        if (!Enum.IsDefined(rol))
            throw new ArgumentException(
                "El rol del usuario no es válido.");

        NombreUsuario = nombreUsuario.Trim();
        NombreCompleto = nombreCompleto.Trim();
        PasswordHash = passwordHash;
        Rol = rol;
        Activo = true;
    }

    public void Desactivar()
    {
        if (!Activo)
            throw new InvalidOperationException(
                "El usuario ya está desactivado.");

        Activo = false;
    }

    public void Activar()
    {
        if (Activo)
            throw new InvalidOperationException(
                "El usuario ya está activo.");

        Activo = true;
    }

    public void CambiarPasswordHash(string nuevoHash)
    {
        if (string.IsNullOrWhiteSpace(nuevoHash))
            throw new ArgumentException(
                "El hash de la contraseña es obligatorio.");

        PasswordHash = nuevoHash;
    }
}
