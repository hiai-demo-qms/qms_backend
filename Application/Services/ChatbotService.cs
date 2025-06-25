using Microsoft.AspNetCore.SignalR.Protocol;
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
        private readonly IDocumentManagement _documentManagement;

        public ChatbotService(QmsDbContext dbContext, HttpClient httpClient, IDocumentManagement documentManagement)
        {
            _dbContext = dbContext;
            _httpClient = httpClient;
            _chatbotApiUrl = "https://ample-wildly-parrot.ngrok-free.app/";
            _documentManagement = documentManagement;
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
                var errorMessage = await response.Content.ReadAsStringAsync();
                return new ApiResponse<AnalyzeDocumentResponse>
                {
                    IsSuccess = false,
                    Message = $"Document analysis failed: {errorMessage}",
                    Response = null,
                    StatusCode = 400
                };
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<AnalyzeDocumentResponse>(responseBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Console.WriteLine($"AnalyzeDocumentAsync result: {responseBody}");

            if (result == null || result.clause_Results == null)
            {
                return new ApiResponse<AnalyzeDocumentResponse>
                {
                    IsSuccess = false,
                    Message = "Phân tích thất bại: kết quả trả về không hợp lệ.",
                    Response = null,
                    StatusCode = 400
                };
            }
            // Lưu vào cơ sở dữ liệu (trừ DocumentId)
            var analyzeEntity = new AnalyzeResponse
            {
                Score = (float)result.score,
                ClauseResults = result.clause_Results.Select(c => new ClauseResult
                {
                    Clause = c.Clause,
                    Title = c.Title,
                    Status = c.Status,
                    Score = (float)c.Score,
                    Evidences = c.Evidences.Select(e => new Evidence
                    {
                        EvidenceText = e
                    }).ToList()
                }).ToList()
            };

            _dbContext.AnalyzeResponses.Add(analyzeEntity);
            var res = await _dbContext.SaveChangesAsync();
            if(res > 0)
            {
                // Trả về lại định dạng yêu cầu
                var responseWithId = new AnalyzeDocumentResponse
                {
                    analyzeResponseId = analyzeEntity.Id,
                    score = analyzeEntity.Score,
                    clause_Results = analyzeEntity.ClauseResults.Select(cr => new ClauseResultsResponse
                    {
                        Clause = cr.Clause,
                        Title = cr.Title,
                        Status = cr.Status,
                        Score = cr.Score,
                        Evidences = cr.Evidences.Select(e => e.EvidenceText).ToList()
                    }).ToList()
                };

                return new ApiResponse<AnalyzeDocumentResponse>
                {
                    IsSuccess = true,
                    Message = "Document analyzed and saved successfully",
                    Response = responseWithId,
                    StatusCode = 200
                };
            }
            return new ApiResponse<AnalyzeDocumentResponse>
            {
                IsSuccess = true,
                Message = "Document analyzed and saved failed",
                Response = null,
                StatusCode = 400
            };
        }
        public async Task<ApiResponse<string>> GetChatResponseAsync(string userId, string message)
        {
            var request = new ChatRequest { question = message };
            string reply;

            var labelResponse = await _httpClient.PostAsJsonAsync(_chatbotApiUrl + "get-label", request);
            var labelRes = await labelResponse.Content.ReadFromJsonAsync<ChatResponse>();
            if (labelRes?.response == "document_search")
            {
                string query = message.ToLower();

                var keywords = await ExtractKeywordsAsync(message);

                if (keywords == null || !keywords.Any())
                {
                    reply = "Rất tiếc, tôi không tìm thấy từ khóa để tìm kiếm tài liệu.";
                }
                else
                {
                    var documents = await _dbContext.Documents
                        .Where(d => keywords.Any(k =>
                            d.Title.ToLower().Contains(k) ||
                            d.Description.ToLower().Contains(k) ||
                            d.Code.ToLower().Contains(k)))
                        .Take(3)
                        .ToListAsync();

                    if (!documents.Any())
                    {
                        reply = "Rất tiếc, tôi không tìm thấy tài liệu phù hợp.";
                    }
                    else
                    {
                        reply = "Tôi tìm thấy các tài liệu liên quan:\n";
                        foreach (var doc in documents)
                        {
                            var result = await _documentManagement.GetDocumentUrlAsync(doc.Id);
                            string shortDesc = string.IsNullOrEmpty(doc.Description)
                                ? ""
                                : doc.Description.Length > 100
                                    ? doc.Description.Substring(0, 100) + "..."
                                    : doc.Description;

                            string url = result.IsSuccess ? result.Response : doc.FilePath;
                            reply += $"\n📄 {doc.Title} (Mã: {doc.Code})\n📝 {shortDesc}\n📂 {url}\n";
                        }
                    }
                }
                ChatLog chatLog = new ChatLog
                {
                    UserId = userId,
                    UserMessage = message,
                    BotResponse = reply,
                    CreatedAt = DateTime.UtcNow
                };
                await _dbContext.ChatLogs.AddAsync(chatLog);
                var res = await _dbContext.SaveChangesAsync();
                if (res > 0)
                {
                    return new ApiResponse<string> { IsSuccess = true, Message = "Connect chatbot successfully", Response = chatLog.BotResponse, StatusCode = 201 };
                }
            }
            else if (labelRes?.response == "normal_question")
            {
                // Lấy 5 tin nhắn gần nhất của người dùng
                var history = await _dbContext.ChatLogs
                    .Where(x => x.UserId == userId && !x.IsDeleted)
                    .OrderByDescending(x => x.CreatedAt)
                    .Take(3)
                    .ToListAsync();

                // Tạo context từ lịch sử chat
                string context = string.Join("\n", history
                    .OrderBy(x => x.CreatedAt)
                    .Select(x => $"Người dùng: {x.UserMessage}\nBot: {x.BotResponse}"));


                // gửi:
                ChatRequest requestFull = new ChatRequest { question = $"{context}\nNgười dùng: {message}\nBot: " } ;

                var response = await _httpClient.PostAsJsonAsync(_chatbotApiUrl + "chat", requestFull);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ChatResponse>();
                    if (result != null)
                    {
                        reply = result.response;
                        ChatLog chatLog = new ChatLog
                        {
                            UserId = userId,
                            UserMessage = message,
                            BotResponse = reply,
                            CreatedAt = DateTime.UtcNow
                        };
                        await _dbContext.ChatLogs.AddAsync(chatLog);
                        var res = await _dbContext.SaveChangesAsync();
                        if (res > 0)
                        {
                            return new ApiResponse<string> { IsSuccess = true, Message = "Connect chatbot successfully", Response = chatLog.BotResponse, StatusCode = 201 };
                        }
                    }

                }
            }
            else if (labelRes?.response == "unknown")
            {
                reply = "Xin lỗi, câu hỏi của bạn không thuộc phạm vi quản lý chất lượng tài liệu (QMS). Vui lòng hỏi về các chủ đề liên quan đến quản lý chất lượng tài liệu để tôi có thể hỗ trợ bạn tốt hơn. ";
                return new ApiResponse<string> { IsSuccess = true, Message = "Connect chatbot successfully", Response = reply, StatusCode = 201 };
            }

            return new ApiResponse<string> { IsSuccess = false, Message = "Connect chatbot failed", Response = "Có lỗi xảy ra, vui lòng thử lại sau.", StatusCode = 400 };

        }

        public async Task<List<string>> ExtractKeywordsAsync(string question)
        {
            var request = new { question };
            var response = await _httpClient.PostAsJsonAsync(_chatbotApiUrl + "extract-keywords", request);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<KeywordResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result?.Keywords ?? new List<string>();
            }
            return new List<string>();
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
