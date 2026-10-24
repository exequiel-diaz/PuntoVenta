
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVenta.Domain.Entities;
using SistemaVenta.Infrastructure.Persistence;
using SistemaVenta.Infrastructure.Persistence.Identity;
using SistemaVenta.Web.ViewModels;
using System;
namespace SistemaVenta.Web.Controllers
{
    public class EmpleadosController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUsuarios> _userManager;

        public EmpleadosController(
            AppDbContext context,
            UserManager<AppUsuarios> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Registrar()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registrar(
            RegistroVM registro)
        {
            if (!ModelState.IsValid)
            {
                return View(registro);
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                var persona = new Persona
                {
                    Nombre = registro.Nombre,
                    Apellido = registro.Apellido,
                    DNI = registro.DNI,
                    Direccion = registro.Direccion,
                    Telefono = registro.Telefono,
                    FechaNacimiento = registro.FechaNacimiento

                };

                _context.Personas.Add(persona);
                await _context.SaveChangesAsync();

                var empleado = new Empleado
                {
                    PersonaId = persona.Id,
                    FechaIngreso = DateTime.Today,
                    Puesto = registro.Puesto,
                    Estado = true
                };

                _context.Empleados.Add(empleado);
                await _context.SaveChangesAsync();


                var usuario = new AppUsuarios
                {
                    UserName = registro.Email,
                    Email = registro.Email,
                    Estado = true,
                    EmpleadoId = empleado.Id,
                };

                var resultado = await _userManager.CreateAsync(
                    usuario,
                    registro.Password);

                if (!resultado.Succeeded)
                {
                    await transaction.RollbackAsync();

                    foreach (var error in resultado.Errors)
                    {
                        ModelState.AddModelError(
                            string.Empty,
                            error.Description);
                    }

                    return View(registro);
                }

                await transaction.CommitAsync();

                TempData["Mensaje"] =
                    "Empleado registrado correctamente.";

                return RedirectToAction("Login", "Cuenta");
            }
               catch
               {
                   await transaction.RollbackAsync();

                   ModelState.AddModelError(
                       string.Empty,
                       "Ocurrió un error al registrar el empleado.");

                   return View(registro);
               }

        }
    }
}
