
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaVenta.Application.UseCases.CuentasCorrientes;
using SistemaVenta.Web.Security;
using SistemaVenta.Web.ViewModels.CuentasCorrientes;
using SistemaVenta.Domain.Constants;
using SistemaVenta.Application.Interfaces;
using System.Security.Claims;

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
    private readonly IConfiguracionCreditoRepository _configuracionCredito;

    public CuentaCorrienteController(
        ObtenerCuentaCorriente obtenerCuentaCorriente, 
        AbrirCuentaCorriente abrirCuentaCorriente,
        ModificarLimiteDeuda modificarLimiteDeuda,
        CerrarCuentaCorriente cerrarCuenta,
        ActivarCuentaCorriente activarCuenta,
        SuspenderCuentaCorriente suspenderCuenta,
        RegistrarDebitoCuentaCorriente registrarDebito,
        RegistrarCreditoCuentaCorriente registrarCredito,
        IConfiguracionCreditoRepository configuracionCredito)
    {
        _obtenerCuentaCorriente = obtenerCuentaCorriente;
        _abrirCuentaCorriente = abrirCuentaCorriente;
        _modificarLimiteDeuda = modificarLimiteDeuda;
        _cerrarCuenta = cerrarCuenta;
        _activarCuenta = activarCuenta;
        _suspenderCuenta = suspenderCuenta;
        _registrarCredito = registrarCredito;
        _registrarDebito = registrarDebito;
        _configuracionCredito = configuracionCredito;
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
    [Authorize(Policy = Permisos.RegistrarCobros)]
    public async Task<IActionResult> RegistrarCobro(int clienteId)
    {
        if (clienteId <= 0)
            return BadRequest();

        try
        {
            var cuenta = await _obtenerCuentaCorriente
                .EjecutarAsync(clienteId);

            if (cuenta.Estado ==
                SistemaVenta.Domain.Enums.EstadoCuentaCorriente.Cerrada)
            {
                TempData["MensajeError"] =
                    "No se pueden registrar cobros en una cuenta cerrada.";

                return RedirectToAction(
                    nameof(Detalle),
                    new { clienteId });
            }

            if (cuenta.SaldoActual <= 0)
            {
                TempData["MensajeError"] =
                    "La cuenta no tiene deuda pendiente.";

                return RedirectToAction(
                    nameof(Detalle),
                    new { clienteId });
            }

            var modelo = new RegistrarMovimientoCuentaViewModel
            {
                CuentaId = cuenta.Id,
                ClienteId = cuenta.ClienteId,
                TipoMovimiento = "Credito"
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
    [Authorize(Policy = Permisos.RegistrarCobros)]
    public async Task<IActionResult> RegistrarCobro(
        RegistrarMovimientoCuentaViewModel modelo)
    {
        // El cobro siempre es un crédito.
        // No confiamos en el tipo enviado por el navegador.
        modelo.TipoMovimiento = "Credito";

        ModelState.Remove(nameof(modelo.TipoMovimiento));

        if (!ModelState.IsValid)
            return View(modelo);

        try
        {
            var cuenta = await _obtenerCuentaCorriente
                .EjecutarAsync(modelo.ClienteId);

            if (cuenta.Id != modelo.CuentaId)
                return BadRequest();

            if (cuenta.Estado ==
                SistemaVenta.Domain.Enums.EstadoCuentaCorriente.Cerrada)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "No se pueden registrar cobros en una cuenta cerrada.");

                return View(modelo);
            }

            if (cuenta.SaldoActual <= 0)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "La cuenta no tiene deuda pendiente.");

                return View(modelo);
            }

            if (modelo.Importe > cuenta.SaldoActual)
            {
                ModelState.AddModelError(
                    nameof(modelo.Importe),
                    "El cobro no puede superar la deuda pendiente.");

                return View(modelo);
            }

            await _registrarCredito.EjecutarAsync(
                cuenta.Id,
                modelo.Importe,
                modelo.Descripcion);

            TempData["MensajeExito"] =
                "El cobro se registró correctamente.";

            return RedirectToAction(
                nameof(Detalle),
                new { clienteId = modelo.ClienteId });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(
                string.Empty,
                ex.Message);
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(
                string.Empty,
                ex.Message);
        }

        return View(modelo);
    }


    [HttpGet]
    [Authorize(Roles =Roles.Administrador)]
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
    [Authorize(Roles =Roles.Administrador)]
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
    [Authorize(Roles =Roles.Administrador)]
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
    [Authorize(Roles = Roles.Administrador)]
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
    [Authorize(Roles = Roles.Administrador)]
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
    [Authorize(Roles = Roles.Administrador)]
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
    [Authorize(Roles = Roles.Administrador)]
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



    [HttpGet]
    [Authorize(Policy = Permisos.AbrirCuentaCorriente)]
    public async Task<IActionResult> Abrir(int clienteId)
    {
        if (clienteId <= 0)
            return BadRequest();

        var limite = await _configuracionCredito
            .ObtenerLimitePredeterminadoAsync();

        var modelo = new AbrirCuentaCorrienteViewModel
        {
            ClienteId = clienteId,
            LimiteDeuda = limite
        };

        ViewBag.EsAdministrador =
            User.IsInRole(Roles.Administrador);

        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = Permisos.AbrirCuentaCorriente)]
    public async Task<IActionResult> Abrir(
        AbrirCuentaCorrienteViewModel modelo)
    {
        bool esAdministrador =
            User.IsInRole(Roles.Administrador);

        ViewBag.EsAdministrador = esAdministrador;

        // Para vendedores, ignoramos siempre el
        // importe recibido desde el navegador.
        if (!esAdministrador)
        {
            modelo.LimiteDeuda = await _configuracionCredito
                .ObtenerLimitePredeterminadoAsync();

            ModelState.Remove(nameof(modelo.LimiteDeuda));
        }

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
                string.Empty, ex.Message);
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(
                string.Empty, ex.Message);
        }

        return View(modelo);
    }


}
