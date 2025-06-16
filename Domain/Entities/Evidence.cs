namespace WebApplication1.Domain.Entities
{
    public class Evidence
    {
        public int Id { get; set; }
        public string EvidenceText { get; set; }
        public int ClauseResultId { get; set; }
        public ClauseResult ClauseResult { get; set; }
    }
}
