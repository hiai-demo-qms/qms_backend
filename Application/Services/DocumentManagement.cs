using Azure.Core;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Application.Models;
using WebApplication1.Domain.Entities;
using WebApplication1.Infrastructure.Data;

namespace WebApplication1.Application.Services
{
    public class DocumentManagement : IDocumentManagement
    {

        private readonly QmsDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public DocumentManagement(QmsDbContext dbContext, IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<ApiResponse<string>> DeleteDocumentAsync(int documentId, string userId)
        {
            var document = await _dbContext.Documents.FirstOrDefaultAsync(m => m.Id == documentId);
            if (document == null)
            {
                return new ApiResponse<string>
                {
                    IsSuccess = false,
                    Message = "Document not found.",
                    StatusCode = 404
                };
            }
            if (document.UserId != userId)
            {
                return new ApiResponse<string>
                {
                    IsSuccess = false,
                    Message = "You do not have permission to delete this document.",
                    StatusCode = 403
                };
            }

            if (File.Exists(document.FilePath))
            {
                File.Delete(document.FilePath);
            }

            var savings = _dbContext.DocumentSavings.Where(ds => ds.DocumentId == documentId);
            _dbContext.DocumentSavings.RemoveRange(savings);

            var analyze = _dbContext.AnalyzeResponses
                .Include(a => a.ClauseResults)
                .Where(a => a.DocumentId == documentId);
            foreach (var a in analyze)
            {
                _dbContext.ClauseResults.RemoveRange(a.ClauseResults);
            }
            _dbContext.AnalyzeResponses.RemoveRange(analyze);

            _dbContext.Documents.Remove(document);
            var res = await _dbContext.SaveChangesAsync();
            if (res > 0)
            {
                return new ApiResponse<string> { IsSuccess = true, Message = "Document deleted successfully", StatusCode = 200 };
            }
            return new ApiResponse<string> { IsSuccess = false, Message = "Failed to delete document", StatusCode = 400 };
        }

        public async Task<ApiResponse<List<Category>>> GetCategoriesAsync()
        {
            var categories = _dbContext.Categories.ToListAsync();
            if (categories == null || !categories.Result.Any())
            {
                return new ApiResponse<List<Category>>
                {
                    IsSuccess = false,
                    Message = "No categories found.",
                    StatusCode = 404
                };
            }

            return new ApiResponse<List<Category>>
            {
                IsSuccess = true,
                Message = "Categories retrieved successfully.",
                Response = await categories,
                StatusCode = 200
            };
        }

        public async Task<ApiResponse<Document>> GetDocumentAsync(int documentId, string userId)
        {
            var document = await _dbContext.Documents.FirstOrDefaultAsync(m => m.Id == documentId);
            if (document == null)
            {
                return new ApiResponse<Document>
                {
                    IsSuccess = false,
                    Message = "Document not found.",
                    StatusCode = 404
                };
            }
            return new ApiResponse<Document>
            {
                IsSuccess = true,
                Message = "Document retrieved successfully.",
                Response = document,
                StatusCode = 200
            };
        }

        public async Task<ApiResponse<List<Document>>> GetDocumentsAsync()
        {
            var documents = await _dbContext.Documents.Include(d => d.User).Include(d => d.Category).ToListAsync();
            if (documents == null)
            {
                return new ApiResponse<List<Document>>
                {
                    IsSuccess = false,
                    Message = "No documents found.",
                    StatusCode = 404
                };
            }

            return new ApiResponse<List<Document>>
            {
                IsSuccess = true,
                Message = "Documents retrieved successfully.",
                Response = documents,
                StatusCode = 200
            };

        }

        public async Task<ApiResponse<string>> GetDocumentUrlAsync(int documentId)
        {
            var document = await _dbContext.Documents.FirstOrDefaultAsync(d => d.Id == documentId);
            if (document == null)
            {
                return new ApiResponse<string>
                {
                    IsSuccess = false,
                    Message = "Document not found.",
                    StatusCode = 404
                };
            }

            var url = $"https://localhost:7147/PDFs/{document.FileName}";
            return new ApiResponse<string>
            {
                IsSuccess = true,
                Message = "Document URL retrieved successfully.",
                Response = url,
                StatusCode = 200
            };
        }

        public async Task<ApiResponse<List<Document>>> GetDocumentWithCategoryAsync(int categoryId)
        {
            var documents = await _dbContext.Documents
                .Where(d => d.CategoryId == categoryId)
                .Include(d => d.Category)
                .Include(d => d.User)
                .ToListAsync();

            if (documents == null)
            {
                return new ApiResponse<List<Document>>
                {
                    IsSuccess = false,
                    Message = "No documents found.",
                    StatusCode = 404
                };
            }

            return new ApiResponse<List<Document>>
            {
                IsSuccess = true,
                Message = "Documents retrieved successfully.",
                Response = documents,
                StatusCode = 200
            };
        }

        public async Task<ApiResponse<List<Document>>> GetUserDocumentsAsync(string userId)
        {
            var documents = await _dbContext.Documents
                .Where(d => d.UserId == userId)
                .Include(d => d.Category)
                .Include(d => d.User)
                .ToListAsync();

            if (documents == null)
            {
                return new ApiResponse<List<Document>>
                {
                    IsSuccess = false,
                    Message = "No documents found.",
                    StatusCode = 404
                };
            }

            return new ApiResponse<List<Document>>
            {
                IsSuccess = true,
                Message = "Documents retrieved successfully.",
                Response = documents,
                StatusCode = 200
            };
        }

        public async Task<ApiResponse<Document>> UpdateDocumentAsync(UpdateFileModel file, int documentId, string userId, int analyzeResponseId)
        {
            var document = await _dbContext.Documents.FirstOrDefaultAsync(m => m.Id == documentId);
            if (document == null)
            {
                return new ApiResponse<Document>
                {
                    IsSuccess = false,
                    Message = "Document not found.",
                    StatusCode = 404
                };
            }

            if (document.UserId != userId)
            {
                return new ApiResponse<Document>
                {
                    IsSuccess = false,
                    Message = "You do not have permission to update this document.",
                    StatusCode = 403
                };
            }

            if (file == null)
            {
                return new ApiResponse<Document>
                {
                    IsSuccess = false,
                    Message = "Invalid update request. File metadata is required.",
                    StatusCode = 400
                };
            }

            var category = await _dbContext.Categories.FirstOrDefaultAsync(m => m.CategoryName == file.Category);
            if (category == null)
            {
                return new ApiResponse<Document>
                {
                    IsSuccess = false,
                    Message = "Category not found.",
                    StatusCode = 404
                };
            }

            if (file.FileUpload != null && file.FileUpload.Length > 0)
            {
                var projectDirectory = Directory.GetCurrentDirectory(); // D:\tot_nghiep\WebApplication1
                var folderPath = Path.Combine(projectDirectory, "wwwroot/PDFs");

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileUpload.FileName);
                var filePath = Path.Combine(folderPath, fileName);

                if (!fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    return new ApiResponse<Document>
                    {
                        IsSuccess = false,
                        Message = "Only PDF files are allowed.",
                        StatusCode = 400
                    };
                }

                // Xóa file cũ nếu tồn tại
                if (!string.IsNullOrEmpty(document.FilePath) && File.Exists(document.FilePath))
                {
                    File.Delete(document.FilePath);
                }

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.FileUpload.CopyToAsync(stream);
                }

                document.FileName = fileName;
                document.FilePath = filePath;
            }

            document.Code = file.Code;
            document.UserId = userId;
            document.CategoryId = category.Id;
            document.Title = file.Title;
            document.Description = file.Description;
            document.Version = file.Version;

            var analyzeResponse = await _dbContext.AnalyzeResponses.FirstOrDefaultAsync(a => a.Id == analyzeResponseId);
            if (analyzeResponse != null)
            {
                analyzeResponse.DocumentId = document.Id;
                _dbContext.AnalyzeResponses.Update(analyzeResponse);
            }

            var res = await _dbContext.SaveChangesAsync();
            if (res > 0)
            {
                return new ApiResponse<Document> { IsSuccess = true, Message = "Update document successfully", Response = document, StatusCode = 201 };
            }

            return new ApiResponse<Document> { IsSuccess = false, Message = "Update document failed", Response = document, StatusCode = 400 };
        }

        public async Task<ApiResponse<Document>> UploadDocumentAsync(UploadFileModel file, string userId, int analyzeResponseId)
        {
            if (file == null || file.FileUpload == null || file.FileUpload.Length == 0)
            {
                return new ApiResponse<Document>
                {
                    IsSuccess = false,
                    Message = "File is empty or not provided.",
                    StatusCode = 400
                };
            }
            var category = await _dbContext.Categories.FirstOrDefaultAsync(m => m.CategoryName == file.Category);
            if (category == null)
            {
                return new ApiResponse<Document>
                {
                    IsSuccess = false,
                    Message = "Category not found.",
                    StatusCode = 404
                };
            }
            var projectDirectory = Directory.GetCurrentDirectory();
            var folderPath = Path.Combine(projectDirectory, "wwwroot/PDFs");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileUpload.FileName);
            var filePath = Path.Combine(folderPath, fileName);

            if (!fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                return new ApiResponse<Document>
                {
                    IsSuccess = false,
                    Message = "Only PDF files are allowed.",
                    StatusCode = 400
                };
            }

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.FileUpload.CopyToAsync(stream);
            }

            var document = new Document
            {
                UserId = userId,
                CategoryId = category.Id,
                Code = file.Code ?? "Uncoded",
                Title = file.Title ?? "Untitled",
                Description = file.Description ?? "No description provided",
                Version = file.Version ?? "1.0",
                FileName = fileName,
                FilePath = filePath,
                UploadedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _dbContext.Documents.AddAsync(document);
            var res = await _dbContext.SaveChangesAsync();
            if (res > 0)
            {
                var fullDoc = await _dbContext.Documents
                    .Include(d => d.Category)
                    .Include(d => d.User)
                    .FirstOrDefaultAsync(d => d.Id == document.Id);
                var analyzeResponse = await _dbContext.AnalyzeResponses.FirstOrDefaultAsync(a => a.Id == analyzeResponseId);
                if (analyzeResponse != null)
                {
                    analyzeResponse.DocumentId = document.Id;
                    _dbContext.AnalyzeResponses.Update(analyzeResponse);
                    await _dbContext.SaveChangesAsync();
                }
                return new ApiResponse<Document> { IsSuccess = true, Message = "Upload document successfully", Response = fullDoc, StatusCode = 201 };
            }
            return new ApiResponse<Document> { IsSuccess = false, Message = "Upload document failed", Response = document, StatusCode = 400 };

        }
        public async Task<ApiResponse<List<Document>>> GetBookmarkedDocuments(string userId)
        {
            var documents = await _dbContext.DocumentSavings
                .Where(ds => ds.UserId == userId)
                .Include(ds => ds.Document)
                    .ThenInclude(d => d.Category)
                .Include(ds => ds.Document)
                    .ThenInclude(d => d.User)
                .Select(ds => ds.Document)
                .ToListAsync();
            return new ApiResponse<List<Document>>
            {
                IsSuccess = true,
                Message = "Bookmarked documents retrieved successfully.",
                Response = documents,
                StatusCode = 200
            };
        }
        public async Task<ApiResponse<Document>> CreateBookmarkDocumentAsync(int documentId, string userId)
        {
            var document = await _dbContext.Documents.FirstOrDefaultAsync(m => m.Id == documentId);
            if (document == null)
            {
                return new ApiResponse<Document>
                {
                    IsSuccess = false,
                    Message = "Document not found.",
                    StatusCode = 404
                };
            }
            var existingBookmark = await _dbContext.DocumentSavings
                .FirstOrDefaultAsync(ds => ds.DocumentId == documentId && ds.UserId == userId);
            if (existingBookmark != null)
            {
                return new ApiResponse<Document>
                {
                    IsSuccess = false,
                    Message = "Document is already bookmarked.",
                    StatusCode = 400
                };
            }
            var bookmark = new DocumentSaving
            {
                DocumentId = documentId,
                UserId = userId
            };
            await _dbContext.DocumentSavings.AddAsync(bookmark);
            var res = await _dbContext.SaveChangesAsync();
            if (res > 0)
            {
                return new ApiResponse<Document> { IsSuccess = true, Message = "Document bookmarked successfully", Response = document, StatusCode = 201 };
            }
            return new ApiResponse<Document> { IsSuccess = false, Message = "Failed to bookmark document", Response = document, StatusCode = 400 };
        }
        public async Task<ApiResponse<string>> DeleteBookmarkDocumentAsync(int documentId, string userId)
        {
            var bookmark = await _dbContext.DocumentSavings
                .FirstOrDefaultAsync(ds => ds.DocumentId == documentId && ds.UserId == userId);
            if (bookmark == null)
            {
                return new ApiResponse<string>
                {
                    IsSuccess = false,
                    Message = "Bookmark not found.",
                    StatusCode = 404
                };
            }
            _dbContext.DocumentSavings.Remove(bookmark);
            var res = await _dbContext.SaveChangesAsync();
            if (res > 0)
            {
                return new ApiResponse<string> { IsSuccess = true, Message = "Bookmark deleted successfully", StatusCode = 200 };
            }
            return new ApiResponse<string> { IsSuccess = false, Message = "Failed to delete bookmark", StatusCode = 400 };
        }
    }
}
