namespace WebApplication1.Application.Models
{
    public class ClauseResultsResponse
    {
        public string Clause { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }
        public double Score { get; set; }
        public List<string> Evidence { get; set; }
    }
}
