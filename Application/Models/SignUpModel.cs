using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Application.Models
{
    public class SignUpModel
    {
        [Required(ErrorMessage = "Must have an email")]
        public string Email { get; set; }
        
        [Required(ErrorMessage = "Must have a password")]
        public string Password { get; set; }
        
        [Required(ErrorMessage = "Must have a full name")]
        public string FullName { get; set; }
    }
}
