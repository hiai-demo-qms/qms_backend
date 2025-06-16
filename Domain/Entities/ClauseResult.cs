namespace WebApplication1.Domain.Entities
{
    public class ClauseResult
    {
        public int Id { get; set; }
        public string Clause { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }
        public float Score { get; set; }

        public int AnalyzeResponseId { get; set; }
        public AnalyzeResponse AnalyzeResponse { get; set; }

        public List<Evidence> Evidences { get; set; } = new();
    }
}
