using SistemaVenta.Domain.Enums;

namespace SistemaVenta.Domain.Entities
{
    public abstract class Cliente
    {
        public int Id { get; private set; }

        public string Telefono { get; private set; }
        public string Email { get; private set; }
        public string Direccion { get; private set; }
        public EstadoCliente Estado { get; private set; }
        public string? Observaciones { get; private set; }

        protected Cliente()
        {
            Telefono = string.Empty;
            Email = string.Empty;
            Direccion = string.Empty;
        }

        protected Cliente(
            string telefono,
            string email,
            string direccion,
            string? observaciones = null)
        {
            ValidarDatosContacto(
                telefono,
                email,
                direccion);

            Telefono = telefono.Trim();
            Email = email.Trim();
            Direccion = direccion.Trim();
            Observaciones = observaciones?.Trim();
            Estado = EstadoCliente.Activo;
        }

        public bool EstaActivo()
        {
            return Estado == EstadoCliente.Activo;
        }

        public void Desactivar()
        {
            Estado = EstadoCliente.Inactivo;
        }

        public void Activar()
        {
            Estado = EstadoCliente.Activo;
        }

        public void ActualizarDatosContacto(
            string telefono,
            string email,
            string direccion,
            string? observaciones)
        {
            ValidarDatosContacto(
                telefono,
                email,
                direccion);

            Telefono = telefono.Trim();
            Email = email.Trim();
            Direccion = direccion.Trim();
            Observaciones = observaciones?.Trim();
        }

        public static void ValidarDatosContacto(
            string telefono,
            string email,
            string direccion)
        {
            if (string.IsNullOrWhiteSpace(telefono))
                throw new ArgumentException(
                    "El teléfono es obligatorio.",
                    nameof(telefono));

            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException(
                    "El correo electrónico es obligatorio.",
                    nameof(email));

            if (string.IsNullOrWhiteSpace(direccion))
                throw new ArgumentException(
                    "La dirección es obligatoria.",
                    nameof(direccion));
        }

    }
}
