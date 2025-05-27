namespace WebApplication1.Application.Models
{
    public class UserInfoModel
    {
        public string Id { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        //public bool TwoFactorEnabled { get; set; }
        public string UserName { get; set; } = null!;
        public string Role { get; set; } = null!;
    }
}
