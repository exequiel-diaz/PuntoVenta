
using SistemaVenta.Domain.ValueObjects;

namespace SistemaVenta.Domain.Entities
{
    public class ClienteEmpresa : Cliente
    {
        public string RazonSocial { get; private set; }
        public string Cuit { get; private set; }
        public string? Contacto { get; private set; }

        private ClienteEmpresa()
        {
            RazonSocial = string.Empty;
            Cuit = string.Empty;
        }

        public ClienteEmpresa(
            string razonSocial,
            string cuit,
            string telefono,
            string email,
            string direccion,
            string? contacto = null,
            string? observaciones = null)
            : base(
                telefono,
                email,
                direccion,
                observaciones)
        {
            ValidarRazonSocial(razonSocial);

            var cuitNormalizado =
                NormalizadorDocumento.NormalizarCuit(cuit);

            RazonSocial = razonSocial.Trim();
            Cuit = cuitNormalizado;

            Contacto = string.IsNullOrWhiteSpace(contacto)
                ? null
                : contacto.Trim();
        }

        public void ActualizarDatosEmpresa(
            string razonSocial,
            string cuit,
            string? contacto)
        {
            ValidarRazonSocial(razonSocial);

            var cuitNormalizado =
                NormalizadorDocumento.NormalizarCuit(cuit);

            RazonSocial = razonSocial.Trim();
            Cuit = cuitNormalizado;

            Contacto = string.IsNullOrWhiteSpace(contacto)
                ? null
                : contacto.Trim();
        }

        private static void ValidarRazonSocial(
            string razonSocial)
        {
            if (string.IsNullOrWhiteSpace(razonSocial))
                throw new ArgumentException(
                    "La razón social es obligatoria.",
                    nameof(razonSocial));
        }
    }
}
