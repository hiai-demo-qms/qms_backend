namespace WebApplication1.Application.Models
{
    public class AuthResponse
    {
        public TokenType AccessToken { get; set; } = null!;
        public TokenType RefreshToken { get; set; } = null!;
    }
}
