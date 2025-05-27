namespace WebApplication1.Domain.Entities
{
    public class Document
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string UserId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
