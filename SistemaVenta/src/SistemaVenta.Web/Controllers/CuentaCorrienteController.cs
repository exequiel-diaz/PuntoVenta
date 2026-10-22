
using Microsoft.AspNetCore.Mvc;
using SistemaVenta.Application.UseCases.CuentasCorrientes;
using SistemaVenta.Web.ViewModels.CuentasCorrientes;

namespace SistemaVenta.Web.Controllers;

public class CuentaCorrienteController : Controller
{
    private readonly ObtenerCuentaCorriente _obtenerCuentaCorriente;
    private readonly AbrirCuentaCorriente _abrirCuentaCorriente;
    private readonly ModificarLimiteDeuda _modificarLimiteDeuda;
    private readonly SuspenderCuentaCorriente _suspenderCuenta;
    private readonly ActivarCuentaCorriente _activarCuenta;
    private readonly CerrarCuentaCorriente _cerrarCuenta;
    private readonly RegistrarDebitoCuentaCorriente _registrarDebito;
    private readonly RegistrarCreditoCuentaCorriente _registrarCredito;

    public CuentaCorrienteController(
        ObtenerCuentaCorriente obtenerCuentaCorriente, 
        AbrirCuentaCorriente abrirCuentaCorriente,
        ModificarLimiteDeuda modificarLimiteDeuda,
        CerrarCuentaCorriente cerrarCuenta,
        ActivarCuentaCorriente activarCuenta,
        SuspenderCuentaCorriente suspenderCuenta,
        RegistrarDebitoCuentaCorriente registrarDebito,
        RegistrarCreditoCuentaCorriente registrarCredito)
    {
        _obtenerCuentaCorriente = obtenerCuentaCorriente;
        _abrirCuentaCorriente = abrirCuentaCorriente;
        _modificarLimiteDeuda = modificarLimiteDeuda;
        _cerrarCuenta = cerrarCuenta;
        _activarCuenta = activarCuenta;
        _suspenderCuenta = suspenderCuenta;
        _registrarCredito = registrarCredito;
        _registrarDebito = registrarDebito;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int clienteId)
    {
        if (clienteId <= 0)
            return BadRequest();

        try
        {
            var cuenta = await _obtenerCuentaCorriente
                .EjecutarAsync(clienteId);

            return RedirectToAction(
                nameof(Detalle),
                new { clienteId = cuenta.ClienteId });
        }
        catch (InvalidOperationException ex)
            when (ex.Message == "El cliente no tiene cuenta corriente.")
        {
            return RedirectToAction(
                nameof(Abrir),
                new { clienteId });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Detalle(int clienteId)
    {
        if (clienteId <= 0)
            return BadRequest();

        try
        {
            var cuenta = await _obtenerCuentaCorriente
                .EjecutarAsync(clienteId);

            return View(cuenta);
        }
        catch (InvalidOperationException ex)
            when (ex.Message == "El cliente no tiene cuenta corriente.")
        {
            return NotFound(
                "El cliente no tiene una cuenta corriente.");
        }
    }

    [HttpGet]
    public IActionResult Abrir(int clienteId)
    {
        if (clienteId <= 0)
            return BadRequest();

        var modelo = new AbrirCuentaCorrienteViewModel
        {
            ClienteId = clienteId
        };

        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Abrir(
        AbrirCuentaCorrienteViewModel modelo)
    {
        if (!ModelState.IsValid)
            return View(modelo);

        try
        {
            await _abrirCuentaCorriente.EjecutarAsync(
                modelo.ClienteId,
                modelo.LimiteDeuda,
                modelo.Observaciones);

            return RedirectToAction(
                nameof(Detalle),
                new { clienteId = modelo.ClienteId });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(
                string.Empty,
                ex.Message);

            return View(modelo);
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(
                string.Empty,
                ex.Message);

            return View(modelo);
        }
    }


    [HttpGet]
    public async Task<IActionResult> ModificarLimite(int clienteId)
    {
        if (clienteId <= 0)
            return BadRequest();

        try
        {
            var cuenta = await _obtenerCuentaCorriente
                .EjecutarAsync(clienteId);

            var modelo = new ModificarLimiteDeudaViewModel
            {
                CuentaId = cuenta.Id,
                ClienteId = cuenta.ClienteId,
                NuevoLimite = cuenta.LimiteDeuda
            };

            return View(modelo);
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ModificarLimite(
        ModificarLimiteDeudaViewModel modelo)
    {
        if (!ModelState.IsValid)
            return View(modelo);

        try
        {
            // Verificamos que la cuenta corresponda al cliente
            // antes de modificarla.
            var cuenta = await _obtenerCuentaCorriente
                .EjecutarAsync(modelo.ClienteId);

            if (cuenta.Id != modelo.CuentaId)
                return BadRequest();

            await _modificarLimiteDeuda.EjecutarAsync(
                modelo.CuentaId,
                modelo.NuevoLimite);

            TempData["MensajeExito"] =
                "Límite de deuda actualizado correctamente.";

            return RedirectToAction(
                nameof(Detalle),
                new { clienteId = modelo.ClienteId });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(modelo);
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(modelo);
        }
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Suspender(
        int cuentaId,
        int clienteId)
    {
        if (cuentaId <= 0 || clienteId <= 0)
            return BadRequest();

        try
        {
            var cuenta = await _obtenerCuentaCorriente
                .EjecutarAsync(clienteId);

            if (cuenta.Id != cuentaId)
                return BadRequest();

            await _suspenderCuenta.EjecutarAsync(cuentaId);

            TempData["MensajeExito"] =
                "Cuenta corriente suspendida correctamente.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["MensajeError"] = ex.Message;
        }
        catch (ArgumentException ex)
        {
            TempData["MensajeError"] = ex.Message;
        }

        return RedirectToAction(
            nameof(Detalle),
            new { clienteId });
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Activar(
        int cuentaId,
        int clienteId)
    {
        if (cuentaId <= 0 || clienteId <= 0)
            return BadRequest();

        try
        {
            var cuenta = await _obtenerCuentaCorriente
                .EjecutarAsync(clienteId);

            if (cuenta.Id != cuentaId)
                return BadRequest();

            await _activarCuenta.EjecutarAsync(cuentaId);

            TempData["MensajeExito"] =
                "Cuenta corriente activada correctamente.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["MensajeError"] = ex.Message;
        }
        catch (ArgumentException ex)
        {
            TempData["MensajeError"] = ex.Message;
        }

        return RedirectToAction(
            nameof(Detalle),
            new { clienteId });
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cerrar(
        int cuentaId,
        int clienteId)
    {
        if (cuentaId <= 0 || clienteId <= 0)
            return BadRequest();

        try
        {
            var cuenta = await _obtenerCuentaCorriente
                .EjecutarAsync(clienteId);

            if (cuenta.Id != cuentaId)
                return BadRequest();

            await _cerrarCuenta.EjecutarAsync(cuentaId);

            TempData["MensajeExito"] =
                "Cuenta corriente cerrada correctamente.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["MensajeError"] = ex.Message;
        }
        catch (ArgumentException ex)
        {
            TempData["MensajeError"] = ex.Message;
        }

        return RedirectToAction(
            nameof(Detalle),
            new { clienteId });
    }


    [HttpGet]
    public async Task<IActionResult> RegistrarMovimiento(
        int clienteId,
        string tipo)
    {
        if (clienteId <= 0)
            return BadRequest();

        if (tipo != "Debito" && tipo != "Credito")
            return BadRequest();

        try
        {
            var cuenta = await _obtenerCuentaCorriente
                .EjecutarAsync(clienteId);

            var modelo = new RegistrarMovimientoCuentaViewModel
            {
                CuentaId = cuenta.Id,
                ClienteId = cuenta.ClienteId,
                TipoMovimiento = tipo
            };

            return View(modelo);
        }
        catch (InvalidOperationException ex)
            when (ex.Message == "El cliente no tiene cuenta corriente.")
        {
            return NotFound();
        }
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegistrarMovimiento(
        RegistrarMovimientoCuentaViewModel modelo)
    {
        if (modelo.TipoMovimiento != "Debito"
            && modelo.TipoMovimiento != "Credito")
        {
            ModelState.AddModelError(
                nameof(modelo.TipoMovimiento),
                "El tipo de movimiento no es válido.");
        }

        if (!ModelState.IsValid)
            return View(modelo);

        try
        {
            var cuenta = await _obtenerCuentaCorriente
                .EjecutarAsync(modelo.ClienteId);

            if (cuenta.Id != modelo.CuentaId)
                return BadRequest();

            if (modelo.TipoMovimiento == "Debito")
            {
                await _registrarDebito.EjecutarAsync(
                    modelo.CuentaId,
                    modelo.Importe,
                    modelo.Descripcion);
            }
            else
            {
                await _registrarCredito.EjecutarAsync(
                    modelo.CuentaId,
                    modelo.Importe,
                    modelo.Descripcion);
            }

            TempData["MensajeExito"] =
                "Movimiento registrado correctamente.";

            return RedirectToAction(
                nameof(Detalle),
                new { clienteId = modelo.ClienteId });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(modelo);
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(modelo);
        }
    }


}
