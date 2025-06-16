namespace WebApplication1.Domain.Entities
{
    public class DocumentSaving
    {
        public int Id { get; set; }
        public int DocumentId { get; set; }
        public string UserId { get; set; }

        public virtual Document Document { get; set; }
        public virtual User User { get; set; }
    }
}
