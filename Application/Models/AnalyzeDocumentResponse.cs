using WebApplication1.Domain.Entities;

namespace WebApplication1.Application.Models
{
    public class AnalyzeDocumentResponse
    {
        public int analyzeResponseId { get; set; }
        public double score { get; set; }
        public List<ClauseResultsResponse> clause_Results { get; set; }
    }
}
