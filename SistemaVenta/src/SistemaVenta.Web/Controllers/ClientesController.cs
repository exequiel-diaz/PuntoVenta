
using Microsoft.AspNetCore.Mvc;
using SistemaVenta.Application.UseCases.Clientes;
using SistemaVenta.Web.Models.Clientes;

namespace SistemaVenta.Web.Controllers;

public class ClientesController : Controller
{
    private readonly ListarClientes _listarClientes;
    private readonly CrearClienteFisico _crearClienteFisico;
    private readonly CrearClienteEmpresa _crearClienteEmpresa;
    private readonly ObtenerCliente _obtenerCliente;
    private readonly EditarClienteFisico _editarClienteFisico;
    private readonly EditarClienteEmpresa _editarClienteEmpresa;
    private readonly ActivarCliente _activarCliente;
    private readonly DesactivarCliente _desactivarCliente;
    public ClientesController(
        ListarClientes listarClientes,
        CrearClienteFisico crearClienteFisico,
        CrearClienteEmpresa crearClienteEmpresa,
        ObtenerCliente obtenerCliente,
        EditarClienteFisico editarClienteFisico,
        EditarClienteEmpresa editarClienteEmpresa,
        ActivarCliente activarCliente,
        DesactivarCliente desactivarCliente)
    {
        _listarClientes = listarClientes;
        _crearClienteFisico = crearClienteFisico;
        _crearClienteEmpresa = crearClienteEmpresa;
        _obtenerCliente = obtenerCliente;
        _editarClienteFisico = editarClienteFisico;
        _editarClienteEmpresa = editarClienteEmpresa;
        _activarCliente = activarCliente;
        _desactivarCliente = desactivarCliente;
    }

    public async Task<IActionResult> Index()
    {
        var clientes = await _listarClientes.EjecutarAsync();
        return View(clientes);
    }

    [HttpGet]
    public IActionResult CrearFisico()
    {
        return View(new CrearClienteFisicoViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearFisico(
        CrearClienteFisicoViewModel modelo)
    {
        if (!ModelState.IsValid)
            return View(modelo);

        try
        {
            await _crearClienteFisico.EjecutarAsync(
                nombre: modelo.Nombre,
                apellido: modelo.Apellido,
                dni: modelo.Dni,
                telefono: modelo.Telefono,
                email: modelo.Email,
                direccion: modelo.Direccion,
                cuit: modelo.Cuit,
                fechaNacimiento: modelo.FechaNacimiento,
                observaciones: modelo.Observaciones
            );

            TempData["MensajeExito"] =
                "Cliente registrado correctamente.";

            return RedirectToAction(nameof(Index));
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
    public IActionResult CrearEmpresa()
    {
        return View(new CrearClienteEmpresaViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearEmpresa(
        CrearClienteEmpresaViewModel modelo)
    {
        if (!ModelState.IsValid)
            return View(modelo);

        try
        {
            await _crearClienteEmpresa.EjecutarAsync(
                razonSocial: modelo.RazonSocial,
                cuit: modelo.Cuit,
                telefono: modelo.Telefono,
                email: modelo.Email,
                direccion: modelo.Direccion,
                contacto: modelo.Contacto,
                observaciones: modelo.Observaciones
            );

            TempData["MensajeExito"] =
                "Cliente empresa registrado correctamente.";

            return RedirectToAction(nameof(Index));
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
    public async Task<IActionResult> EditarFisico(int id)
    {
        try
        {
            var cliente = await _obtenerCliente.EjecutarAsync(id);

            if (cliente.Tipo != "Físico")
                return NotFound();

            var modelo = new EditarClienteFisicoViewModel
            {
                Id = cliente.Id,
                Nombre = cliente.Nombre ?? string.Empty,
                Apellido = cliente.Apellido ?? string.Empty,
                Dni = cliente.Dni ?? string.Empty,
                Cuit = cliente.Cuit,
                FechaNacimiento = cliente.FechaNacimiento,
                Telefono = cliente.Telefono,
                Email = cliente.Email,
                Direccion = cliente.Direccion,
                Observaciones = cliente.Observaciones
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
    public async Task<IActionResult> EditarFisico(
        int id,
        EditarClienteFisicoViewModel modelo)
    {
        if (id != modelo.Id)
            return BadRequest();

        if (!ModelState.IsValid)
            return View(modelo);

        try
        {
            await _editarClienteFisico.EjecutarAsync(
                id: modelo.Id,
                nombre: modelo.Nombre,
                apellido: modelo.Apellido,
                dni: modelo.Dni,
                telefono: modelo.Telefono,
                email: modelo.Email,
                direccion: modelo.Direccion,
                cuit: modelo.Cuit,
                fechaNacimiento: modelo.FechaNacimiento,
                observaciones: modelo.Observaciones
            );

            TempData["MensajeExito"] =
                "Cliente actualizado correctamente.";

            return RedirectToAction(nameof(Index));
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
    public async Task<IActionResult> EditarEmpresa(int id)
    {
        try
        {
            var cliente = await _obtenerCliente.EjecutarAsync(id);

            if (cliente.Tipo != "Empresa")
                return NotFound();

            var modelo = new EditarClienteEmpresaViewModel
            {
                Id = cliente.Id,
                RazonSocial = cliente.RazonSocial ?? string.Empty,
                Cuit = cliente.Cuit ?? string.Empty,
                Telefono = cliente.Telefono,
                Email = cliente.Email,
                Direccion = cliente.Direccion,
                Contacto = cliente.Contacto,
                Observaciones = cliente.Observaciones
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
    public async Task<IActionResult> EditarEmpresa(
        int id,
        EditarClienteEmpresaViewModel modelo)
    {
        if (id != modelo.Id)
            return BadRequest();

        if (!ModelState.IsValid)
            return View(modelo);

        try
        {
            await _editarClienteEmpresa.EjecutarAsync(
                id: modelo.Id,
                razonSocial: modelo.RazonSocial,
                cuit: modelo.Cuit,
                telefono: modelo.Telefono,
                email: modelo.Email,
                direccion: modelo.Direccion,
                contacto: modelo.Contacto,
                observaciones: modelo.Observaciones
            );

            TempData["MensajeExito"] =
                "Empresa actualizada correctamente.";

            return RedirectToAction(nameof(Index));
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
    public async Task<IActionResult> Activar(int id)
    {
        try
        {
            await _activarCliente.EjecutarAsync(id);

            TempData["MensajeExito"] =
                "Cliente activado correctamente.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["MensajeError"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Desactivar(int id)
    {
        try
        {
            await _desactivarCliente.EjecutarAsync(id);

            TempData["MensajeExito"] =
                "Cliente desactivado correctamente.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["MensajeError"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }


}
