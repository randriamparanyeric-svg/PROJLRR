using System.ComponentModel.DataAnnotations;

namespace PROJLRR.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Le nom d'utilisateur est requis.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Le mot de passe est requis.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}