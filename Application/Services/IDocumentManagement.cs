using WebApplication1.Application.Models;
using WebApplication1.Domain.Entities;

namespace WebApplication1.Application.Services
{
    public interface IDocumentManagement
    {
        public Task<ApiResponse<Document>> UploadDocumentAsync(UploadFileModel file, string userId);
        public Task<ApiResponse<string>> DeleteDocumentAsync(int documentId, string userId);
        public Task<ApiResponse<List<Document>>> GetUserDocumentsAsync(string userId);
        public Task<ApiResponse<string>> GetDocumentUrlAsync(int documentId, string userId);
        public Task<ApiResponse<Document>> UpdateDocumentAsync(UploadFileModel file, int documentId, string userId);
        public Task<ApiResponse<List<Document>>> GetDocumentWithCategoryAsync(int categoryId);
        public Task<ApiResponse<List<Document>>> GetDocumentsAsync();
        public Task<ApiResponse<Document>> GetDocumentAsync(int documentId, string userId);
        public Task<ApiResponse<List<Category>>> GetCategoriesAsync();
    }
}
