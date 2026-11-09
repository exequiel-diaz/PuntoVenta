
using SistemaVenta.Application.DTOs.CuentasCorrientes;
using SistemaVenta.Application.Interfaces;
using SistemaVenta.Domain.Entities;

namespace SistemaVenta.Application.UseCases.CuentasCorrientes;

public class ListarCuentasCorrientes
{
    private readonly ICuentaCorrienteRepository _cuentas;
    private readonly IClienteRepository _clientes;

    public ListarCuentasCorrientes(
        ICuentaCorrienteRepository cuentas,
        IClienteRepository clientes)
    {
        _cuentas = cuentas;
        _clientes = clientes;
    }

    public async Task<IReadOnlyList<CuentaCorrienteResumenDto>>
        EjecutarAsync()
    {
        var cuentas = await _cuentas.ObtenerTodasAsync();

        var resultado = new List<CuentaCorrienteResumenDto>();

        foreach (var cuenta in cuentas)
        {
            var cliente = await _clientes.ObtenerPorIdAsync(
                cuenta.ClienteId);

            string nombre = "Cliente no encontrado";
            string documento = string.Empty;

            switch (cliente)
            {
                case ClienteFisico fisico:
                    nombre = $"{fisico.Nombre} {fisico.Apellido}";
                    documento = fisico.Dni;
                    break;

                case ClienteEmpresa empresa:
                    nombre = empresa.RazonSocial;
                    documento = empresa.Cuit;
                    break;
            }

            resultado.Add(new CuentaCorrienteResumenDto
            {
                Id = cuenta.Id,
                ClienteId = cuenta.ClienteId,
                Cliente = nombre,
                Documento = documento,
                SaldoActual = cuenta.SaldoActual,
                LimiteDeuda = cuenta.LimiteDeuda,
                Estado = cuenta.Estado.ToString()
            });
        }

        return resultado
            .OrderBy(c => c.Cliente)
            .ToList();
    }
}
