namespace WebApplication1.Domain.Entities
{
    public class AnalyzeResponse
    {
        public int Id { get; set; }
        public float Score { get; set; }
        public int DocumentId { get; set; }
        public List<ClauseResult> ClauseResults { get; set; } = new();
    }
}
