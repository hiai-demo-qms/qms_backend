using WebApplication1.Application.Models;
using WebApplication1.Domain.Entities;

namespace WebApplication1.Application.Services
{
    public interface IChatbotService
    {
        Task<ApiResponse<AnalyzeDocumentResponse>> AnalyzeDocumentAsync(IFormFile file);
        Task<ApiResponse<string>> GetChatResponseAsync(string userId, string message);
        Task<ApiResponse<List<ChatLog>>> GetChatLogsAsync(string userId);
        Task<ApiResponse<string>> SaveChatLogAsync(ChatLog chatLog);
        Task<ApiResponse<string>> DeleteChatLogAsync(int chatLogId, string userId);
        Task<ApiResponse<List<Domain.Entities.AnalyzeResponse>>> GetAnalyzeResponsesAsync(int documentId, string userId);
    }
}
