using WebApplication1.Domain.Entities;

namespace WebApplication1.Application.Models
{
    public class AnalyzeDocumentResponse
    {
        public double Score { get; set; }
        public List<ClauseResultsResponse> Clause_Results { get; set; }
    }
}
