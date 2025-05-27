namespace WebApplication1.Domain.Entities
{
    public class ChatLog
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserMessage { get; set; } = string.Empty;
        public string BotResponse { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;
    }
}
