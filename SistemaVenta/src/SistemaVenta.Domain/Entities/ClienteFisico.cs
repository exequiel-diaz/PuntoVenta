
using SistemaVenta.Domain.ValueObjects;

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
            ValidarNombreYApellido(nombre, apellido);

            var dniNormalizado =
                NormalizadorDocumento.NormalizarDni(dni);

            var cuitNormalizado =
                NormalizarCuitOpcional(cuit);

            Nombre = nombre.Trim();
            Apellido = apellido.Trim();
            Dni = dniNormalizado;
            Cuit = cuitNormalizado;
            FechaNacimiento = fechaNacimiento;
        }

        public void ActualizarDatosPersonales(
            string nombre,
            string apellido,
            string dni,
            string? cuit,
            DateTime? fechaNacimiento)
        {
            ValidarNombreYApellido(nombre, apellido);

            var dniNormalizado =
                NormalizadorDocumento.NormalizarDni(dni);

            var cuitNormalizado =
                NormalizarCuitOpcional(cuit);

            Nombre = nombre.Trim();
            Apellido = apellido.Trim();
            Dni = dniNormalizado;
            Cuit = cuitNormalizado;
            FechaNacimiento = fechaNacimiento;
        }

        private static void ValidarNombreYApellido(
            string nombre,
            string apellido)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException(
                    "El nombre es obligatorio.",
                    nameof(nombre));

            if (string.IsNullOrWhiteSpace(apellido))
                throw new ArgumentException(
                    "El apellido es obligatorio.",
                    nameof(apellido));
        }

        private static string? NormalizarCuitOpcional(
            string? cuit)
        {
            return string.IsNullOrWhiteSpace(cuit)
                ? null
                : NormalizadorDocumento.NormalizarCuit(cuit);
        }
    }
}
