using Microsoft.AspNetCore.Identity;

namespace WebApplication1.Domain.Entities
{
    public class User : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiration { get; set; }

    }
}
