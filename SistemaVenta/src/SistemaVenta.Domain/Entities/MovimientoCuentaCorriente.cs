using SistemaVenta.Domain.Enums;

namespace SistemaVenta.Domain.Entities;

public class MovimientoCuentaCorriente
{
    public int Id { get; private set; }

    public int CuentaCorrienteId { get; private set; }

    public DateTime Fecha { get; private set; }

    public TipoMovimientoCuenta Tipo { get; private set; }

    public decimal Importe { get; private set; }

    public string Descripcion { get; private set; } = string.Empty;

    // Constructor requerido por EF Core.
    private MovimientoCuentaCorriente()
    {
    }

    internal MovimientoCuentaCorriente(
        TipoMovimientoCuenta tipo,
        decimal importe,
        string descripcion)
    {
        if (!Enum.IsDefined(tipo))
            throw new ArgumentException(
                "El tipo de movimiento no es válido.");

        if (importe <= 0)
            throw new ArgumentException(
                "El importe debe ser mayor que cero.");

        if (string.IsNullOrWhiteSpace(descripcion))
            throw new ArgumentException(
                "La descripción es obligatoria.");

        Tipo = tipo;
        Importe = importe;
        Descripcion = descripcion.Trim();

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