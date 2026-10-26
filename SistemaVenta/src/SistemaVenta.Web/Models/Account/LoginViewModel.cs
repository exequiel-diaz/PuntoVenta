
using System.ComponentModel.DataAnnotations;

namespace SistemaVenta.Web.ViewModels.Account;

public class LoginViewModel
{
    [Required(ErrorMessage = "Ingresá tu nombre de usuario.")]
    [Display(Name = "Nombre de usuario")]
    public string NombreUsuario { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresá tu contraseña.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = string.Empty;
}
