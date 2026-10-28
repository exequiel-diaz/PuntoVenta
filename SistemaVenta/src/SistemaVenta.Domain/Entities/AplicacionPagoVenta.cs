namespace SistemaVenta.Domain.Entities;

public class AplicacionPagoVenta
{
    public int Id { get; private set; }

    public int PagoId { get; private set; }

    public int VentaId { get; private set; }

    public decimal ImporteAplicado { get; private set; }

    public Pago Pago { get; private set; } = null!;

    public Venta Venta { get; private set; } = null!;

    // Constructor requerido por EF Core.
    private AplicacionPagoVenta()
    {
    }

    internal AplicacionPagoVenta(
        Pago pago,
        Venta venta,
        decimal importeAplicado)
    {
        if (pago is null)
            throw new ArgumentNullException(nameof(pago));

        if (venta is null)
            throw new ArgumentNullException(nameof(venta));

        if (importeAplicado <= 0)
            throw new ArgumentException(
                "El importe aplicado debe ser mayor que cero.",
                nameof(importeAplicado));

        Pago = pago;
        Venta = venta;
        ImporteAplicado = importeAplicado;
    }
}