using SistemaVenta.Domain.Enums;

namespace SistemaVenta.Domain.Entities;

public class CuentaCorriente
{
    public int Id { get; private set; }

    public int ClienteId { get; private set; }

    public decimal LimiteDeuda { get; private set; }

    public DateTime FechaApertura { get; private set; }

    public EstadoCuentaCorriente Estado { get; private set; }

    public string? Observaciones { get; private set; }

    private readonly List<MovimientoCuentaCorriente> _movimientos = new();

    public IReadOnlyCollection<MovimientoCuentaCorriente> Movimientos
        => _movimientos.AsReadOnly();

    public decimal SaldoActual =>
        _movimientos.Sum(m =>
            m.EsDebito() ? m.Importe : -m.Importe);

    // Constructor requerido por EF Core.
    private CuentaCorriente()
    {
    }

    public CuentaCorriente(
        int clienteId,
        decimal limiteDeuda,
        string? observaciones = null)
    {
        if (clienteId <= 0)
            throw new ArgumentException(
                "El cliente debe ser válido.");

        if (limiteDeuda < 0)
            throw new ArgumentException(
                "El límite de deuda no puede ser negativo.");

        ClienteId = clienteId;
        LimiteDeuda = limiteDeuda;
        FechaApertura = DateTime.UtcNow;
        Estado = EstadoCuentaCorriente.Activa;
        Observaciones = observaciones?.Trim();
    }

    public bool PuedeRegistrarDeuda(decimal importe)
    {
        return Estado == EstadoCuentaCorriente.Activa
            && importe > 0
            && SaldoActual + importe <= LimiteDeuda;
    }

    public void RegistrarDebito(
        decimal importe,
        string descripcion,
        Venta? venta = null)
    {
        if (importe <= 0)
            throw new ArgumentException(
                "El importe debe ser mayor que cero.");

        if (!PuedeRegistrarDeuda(importe))
            throw new InvalidOperationException(
                "La cuenta no permite registrar esta deuda.");

        var movimiento = new MovimientoCuentaCorriente(
            TipoMovimientoCuenta.Debito,
            importe,
            descripcion,
            venta);

        _movimientos.Add(movimiento);
    }


    public void RegistrarCredito(
        decimal importe,
        string descripcion,
        Venta? venta = null)
    {
        if (Estado == EstadoCuentaCorriente.Cerrada)
            throw new InvalidOperationException(
                "No se pueden registrar movimientos en una cuenta cerrada.");

        if (importe <= 0)
            throw new ArgumentException(
                "El importe debe ser mayor que cero.");

        if (importe > SaldoActual)
            throw new InvalidOperationException(
                "El crédito no puede superar la deuda existente.");

        var movimiento = new MovimientoCuentaCorriente(
            TipoMovimientoCuenta.Credito,
            importe,
            descripcion,
            venta);

        _movimientos.Add(movimiento);
    }


    public bool TieneDebitoDeVenta(int ventaId)
    {
        if (ventaId <= 0)
            throw new ArgumentException(
                "El identificador de la venta debe ser válido.");

        return _movimientos.Any(m =>
            m.VentaId == ventaId &&
            m.EsDebito());
    }

    public bool TieneCreditoDeVenta(int ventaId)
    {
        if (ventaId <= 0)
            throw new ArgumentException(
                "El identificador de la venta debe ser válido.");

        return _movimientos.Any(m =>
            m.VentaId == ventaId &&
            !m.EsDebito());
    }


    public void RevertirDebitoDeVenta(Venta venta)
    {
        if (venta is null)
            throw new ArgumentNullException(nameof(venta));

        if (venta.Id <= 0)
            throw new ArgumentException(
                "La venta debe estar registrada.");

        if (venta.ClienteId != ClienteId)
            throw new InvalidOperationException(
                "La venta no pertenece al cliente de esta cuenta.");

        var debitos = _movimientos
            .Where(m =>
                m.VentaId == venta.Id &&
                m.EsDebito())
            .ToList();

        if (debitos.Count != 1)
            throw new InvalidOperationException(
                "La venta debe tener exactamente un débito asociado.");

        var tienePagosAplicados = _movimientos.Any(m =>
            m.VentaId == venta.Id &&
            m.EsCredito() &&
            m.PagoId.HasValue);

        if (tienePagosAplicados)
        {
            throw new InvalidOperationException(
                "No se puede anular una venta que tiene pagos aplicados.");
        }

        if (TieneCreditoDeVenta(venta.Id))
            throw new InvalidOperationException(
                "La venta ya tiene un crédito asociado.");

        var importe = debitos[0].Importe;

        if (SaldoActual < importe)
            throw new InvalidOperationException(
                "No se puede anular esta venta porque la cuenta no tiene deuda suficiente para compensarla. Revisá los pagos registrados.");

        RegistrarCredito(
            importe,
            $"Anulación de venta #{venta.Id}",
            venta);
    }


    public void ModificarLimiteDeuda(decimal nuevoLimite)
    {
        if (Estado == EstadoCuentaCorriente.Cerrada)
            throw new InvalidOperationException(
                "No se puede modificar una cuenta cerrada.");

        if (nuevoLimite < 0)
            throw new ArgumentException(
                "El límite de deuda no puede ser negativo.");

        LimiteDeuda = nuevoLimite;
    }

    public void Suspender()
    {
        if (Estado == EstadoCuentaCorriente.Cerrada)
            throw new InvalidOperationException(
                "No se puede suspender una cuenta cerrada.");

        Estado = EstadoCuentaCorriente.Suspendida;
    }

    public void Activar()
    {
        if (Estado == EstadoCuentaCorriente.Cerrada)
            throw new InvalidOperationException(
                "No se puede reactivar una cuenta cerrada.");

        Estado = EstadoCuentaCorriente.Activa;
    }

    public void Cerrar()
    {
        if (SaldoActual != 0)
            throw new InvalidOperationException(
                "No se puede cerrar una cuenta con deuda pendiente.");

        Estado = EstadoCuentaCorriente.Cerrada;
    }

    public void RegistrarPagoDeVenta(
    Venta venta,
    Pago pago,
    decimal importe)
    {
        if (venta is null)
            throw new ArgumentNullException(nameof(venta));

        if (pago is null)
            throw new ArgumentNullException(nameof(pago));

        if (venta.Id <= 0)
            throw new ArgumentException(
                "La venta debe estar registrada.");

        if (venta.ClienteId != ClienteId)
            throw new InvalidOperationException(
                "La venta no pertenece al cliente de esta cuenta.");

        if (pago.Id <= 0)
            throw new ArgumentException(
                "El pago debe estar registrado.");

        if (Estado == EstadoCuentaCorriente.Cerrada)
            throw new InvalidOperationException(
                "No se pueden registrar pagos en una cuenta cerrada.");

        if (importe <= 0)
            throw new ArgumentException(
                "El importe debe ser mayor que cero.");

        if (!TieneDebitoDeVenta(venta.Id))
            throw new InvalidOperationException(
                "La venta no tiene una deuda registrada en esta cuenta.");

        var debitoRevertido = _movimientos.Any(m =>
            m.VentaId == venta.Id &&
            m.EsCredito() &&
            !m.PagoId.HasValue &&
            m.Pago is null);

        if (debitoRevertido)
        {
            throw new InvalidOperationException(
                "No se pueden aplicar pagos a una venta cuyo débito fue revertido.");
        }

        var saldoPendiente =
            ObtenerSaldoPendienteDeVenta(venta.Id);

        if (importe > saldoPendiente)
        {
            throw new InvalidOperationException(
                "El pago no puede superar el saldo pendiente de la venta.");
        }

        if (importe > SaldoActual)
        {
            throw new InvalidOperationException(
                "El pago no puede superar la deuda de la cuenta.");
        }

        var movimiento = new MovimientoCuentaCorriente(
            TipoMovimientoCuenta.Credito,
            importe,
            $"Pago #{pago.Id} aplicado a venta #{venta.Id}",
            venta,
            pago);

        _movimientos.Add(movimiento);
    }

    public decimal ObtenerSaldoPendienteDeVenta(int ventaId)
    {
        if (ventaId <= 0)
        {
            throw new ArgumentException(
                "El identificador de la venta debe ser válido.");
        }

        var debitos = _movimientos
            .Where(m =>
                m.VentaId == ventaId &&
                m.EsDebito())
            .Sum(m => m.Importe);

        var creditos = _movimientos
            .Where(m =>
                m.VentaId == ventaId &&
                m.EsCredito())
            .Sum(m => m.Importe);

        return debitos - creditos;
    }


}