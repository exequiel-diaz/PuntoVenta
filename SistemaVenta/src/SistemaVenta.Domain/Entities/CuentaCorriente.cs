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
        string descripcion)
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
            descripcion);

        _movimientos.Add(movimiento);
    }

    public void RegistrarCredito(
        decimal importe,
        string descripcion)
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
            descripcion);

        _movimientos.Add(movimiento);
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
}