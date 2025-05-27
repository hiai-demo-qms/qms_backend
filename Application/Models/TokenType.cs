namespace WebApplication1.Application.Models
{
    public class TokenType
    {
        public string Token { get; set; } = null!;
        public DateTime ExpirationTokenDate { get; set; }
    }
}
