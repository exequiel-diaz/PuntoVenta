using SistemaVenta.Domain.Enums;

namespace SistemaVenta.Domain.Entities;

public class MovimientoCuentaCorriente
{
    public int Id { get; private set; }

    public int CuentaCorrienteId { get; private set; }
    public int? VentaId { get; private set; }
    public Venta? Venta { get; private set; }
    public int? PagoId { get; private set; }

    public Pago? Pago { get; private set; }
    public int? DocumentoComercialId { get; private set; }

    public DocumentoComercial? DocumentoComercial { get; private set; }
    public DateTime Fecha { get; private set; }

    public TipoMovimientoCuenta Tipo { get; private set; }
    public OrigenMovimientoCuenta Origen { get; private set; }

    public decimal Importe { get; private set; }

    public string Descripcion { get; private set; } = string.Empty;

    // Constructor requerido por EF Core.
    private MovimientoCuentaCorriente()
    {
    }


    internal MovimientoCuentaCorriente(
        TipoMovimientoCuenta tipo,
        decimal importe,
        string descripcion,
        Venta? venta = null,
        Pago? pago = null,
        OrigenMovimientoCuenta origen = OrigenMovimientoCuenta.AjusteManual,
        DocumentoComercial? documentoComercial = null)
    {
        if (!Enum.IsDefined(tipo))
            throw new ArgumentException(
                "El tipo de movimiento no es válido.");

        if (!Enum.IsDefined(origen))
            throw new ArgumentException(
                "El origen del movimiento no es válido.");

        if (importe <= 0)
            throw new ArgumentException(
                "El importe debe ser mayor que cero.");

        if (string.IsNullOrWhiteSpace(descripcion))
            throw new ArgumentException(
                "La descripción es obligatoria.");

        if (pago is not null && tipo != TipoMovimientoCuenta.Credito)
        {
            throw new InvalidOperationException(
                "Un pago solamente puede asociarse a un movimiento de crédito.");
        }

        Tipo = tipo;
        Origen = origen;
        Importe = importe;
        Descripcion = descripcion.Trim();

        Venta = venta;
        VentaId = venta?.Id;


        Pago = pago;
        PagoId = pago?.Id;

        DocumentoComercial = documentoComercial;
        DocumentoComercialId = documentoComercial?.Id;

        Fecha = DateTime.UtcNow;


    }
    public bool EsDebito()
    {
        return Tipo == TipoMovimientoCuenta.Debito;
    }

    public bool EsCredito()
    {
        return Tipo == TipoMovimientoCuenta.Credito;
    }
}