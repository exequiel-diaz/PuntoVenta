

namespace SistemaVenta.Domain.Entities
{
    public class ClienteFisico : Cliente
    {
        public string Nombre { get; private set; }
        public string Apellido { get; private set; }
        public string Dni { get; private set; }
        public string? Cuit { get; private set; }
        public DateTime? FechaNacimiento { get; private set; }

        private ClienteFisico()
        {
            Nombre = string.Empty;
            Apellido = string.Empty;
            Dni = string.Empty;
        }

        public ClienteFisico(
            string nombre,
            string apellido,
            string dni,
            string telefono,
            string email,
            string direccion,
            string? cuit = null,
            DateTime? fechaNacimiento = null,
            string? observaciones = null)
            : base(
                telefono,
                email,
                direccion,
                observaciones)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException(
                    "El nombre es obligatorio.",
                    nameof(nombre));

            if (string.IsNullOrWhiteSpace(apellido))
                throw new ArgumentException(
                    "El apellido es obligatorio.",
                    nameof(apellido));

            if (string.IsNullOrWhiteSpace(dni))
                throw new ArgumentException(
                    "El DNI es obligatorio.",
                    nameof(dni));

            Nombre = nombre.Trim();
            Apellido = apellido.Trim();
            Dni = dni.Trim();
            Cuit = string.IsNullOrWhiteSpace(cuit)
                ? null
                : cuit.Trim();

            FechaNacimiento = fechaNacimiento;
        }

        public void ActualizarDatosPersonales(
            string nombre,
            string apellido,
            string dni,
            string? cuit,
            DateTime? fechaNacimiento)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException(
                    "El nombre es obligatorio.",
                    nameof(nombre));

            if (string.IsNullOrWhiteSpace(apellido))
                throw new ArgumentException(
                    "El apellido es obligatorio.",
                    nameof(apellido));

            if (string.IsNullOrWhiteSpace(dni))
                throw new ArgumentException(
                    "El DNI es obligatorio.",
                    nameof(dni));

            Nombre = nombre.Trim();
            Apellido = apellido.Trim();
            Dni = dni.Trim();

            Cuit = string.IsNullOrWhiteSpace(cuit)
                ? null
                : cuit.Trim();

            FechaNacimiento = fechaNacimiento;
        }

    }
}
