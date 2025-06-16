using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Reflection.Metadata;
using System.Text.Json;
using WebApplication1.Application.Models;
using WebApplication1.Domain.Entities;
using WebApplication1.Infrastructure.Data;

namespace WebApplication1.Application.Services
{

    public class ChatbotService: IChatbotService
    {
        private readonly QmsDbContext _dbContext;
        private readonly IChatbotService _chatbotService;
        private readonly HttpClient _httpClient;
        private readonly string _chatbotApiUrl;

        public ChatbotService(QmsDbContext dbContext, HttpClient httpClient)
        {
            _dbContext = dbContext;
            _httpClient = httpClient;
            _chatbotApiUrl = "https://ample-wildly-parrot.ngrok-free.app/";
        }
        public async Task<ApiResponse<AnalyzeDocumentResponse>> AnalyzeDocumentAsync(IFormFile file)
        {
            using var content = new MultipartFormDataContent();

            using var stream = file.OpenReadStream();
            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            content.Add(fileContent, "document", file.FileName);

            var response = await _httpClient.PostAsync(_chatbotApiUrl + "analyze", content);

            if (!response.IsSuccessStatusCode)
            {
                return new ApiResponse<AnalyzeDocumentResponse>
                {
                    IsSuccess = false,
                    Message = "Document analyzed failed",
                    Response = null,
                    StatusCode = 400
                };
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<AnalyzeDocumentResponse>(responseBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return new ApiResponse<AnalyzeDocumentResponse>
            {
                IsSuccess = true,
                Message = "Document analyzed successfully",
                Response = result!,
                StatusCode = 200
            };
        }
        public async Task<ApiResponse<string>> GetChatResponseAsync(string userId, string message)
        {
            var request = new ChatRequest { question = message };

            var response = await _httpClient.PostAsJsonAsync(_chatbotApiUrl + "chat", request);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ChatResponse>();
                if (result != null)
                {
                    ChatLog chatLog = new ChatLog
                    {
                        UserId = userId,
                        UserMessage = message,
                        BotResponse = result.response,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _dbContext.ChatLogs.AddAsync(chatLog);
                    var res = await _dbContext.SaveChangesAsync();
                    if (res > 0)
                    {
                        return new ApiResponse<string> { IsSuccess = true, Message = "Connect chatbot successfully", Response = chatLog.BotResponse, StatusCode = 201 };
                    }
                    return new ApiResponse<string> { IsSuccess = false, Message = "Connect chatbot failed", Response = "Có lỗi xảy ra, vui lòng thử lại sau.", StatusCode = 400 };
                }
                
            }

            return new ApiResponse<string> { IsSuccess = false, Message = "Connect chatbot failed", Response = "Có lỗi xảy ra, vui lòng thử lại sau.", StatusCode = 400 };
        }
        public async Task<ApiResponse<List<ChatLog>>> GetChatLogsAsync(string userId)
        {
            return await _chatbotService.GetChatLogsAsync(userId);
        }
        public async Task<ApiResponse<string>> SaveChatLogAsync(ChatLog chatLog)
        {
            return await _chatbotService.SaveChatLogAsync(chatLog);
        }
        public async Task<ApiResponse<string>> DeleteChatLogAsync(int chatLogId, string userId)
        {
            return await _chatbotService.DeleteChatLogAsync(chatLogId, userId);
        }
        public async Task<ApiResponse<List<Domain.Entities.AnalyzeResponse>>> GetAnalyzeResponsesAsync(int documentId, string userId)
        {
            return await _chatbotService.GetAnalyzeResponsesAsync(documentId, userId);
        }
    }
}
