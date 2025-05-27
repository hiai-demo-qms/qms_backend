using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Application.Models
{
    public class SignInModel
    {
        [Required(ErrorMessage = "Must have an email")]
        public string Email { get; set; } = null!;
        [Required(ErrorMessage = "Must have an user")]
        public string Password { get; set; } = null!;
    }
}
