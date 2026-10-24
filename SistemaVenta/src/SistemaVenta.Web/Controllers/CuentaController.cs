using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SistemaVenta.Infrastructure.Persistence.Identity;
using SistemaVenta.Web.ViewModels;

namespace SistemaVenta.Web.Controllers
{
    public class CuentaController : Controller
    {
        private readonly SignInManager<AppUsuarios> _signInManager;
        private readonly UserManager<AppUsuarios> _userManager;
        public CuentaController(
            SignInManager<AppUsuarios> signInManager, UserManager<AppUsuarios> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM vm)
        {

            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var usuario = await _userManager.FindByEmailAsync(vm.Email);

            if (usuario == null || !usuario.Estado)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Correo o contraseña incorrectos.");

                return View(vm);
            }

            var resultado = await _signInManager.PasswordSignInAsync(
                usuario.UserName!,
                vm.Password,
                isPersistent: false,
                lockoutOnFailure: false);

            if (resultado.Succeeded)
            {
                usuario.UltimoAcceso = DateTime.UtcNow;
                await _userManager.UpdateAsync(usuario);

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(
                string.Empty,
                "Correo o contraseña incorrectos.");

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction("Inicio", "Home");
        }
    }
}
