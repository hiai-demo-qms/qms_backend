namespace WebApplication1.Application.Models
{
    public class ChangePasswordModel
    {
        public string OldPassword { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
    }
}
