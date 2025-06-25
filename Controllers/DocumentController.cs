using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using WebApplication1.Application.Models;
using WebApplication1.Application.Services;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class DocumentController : ControllerBase
    {
        private readonly IDocumentManagement _documentManager;
        private readonly IUserManagement _userManager;

        public DocumentController(IDocumentManagement documentManager, IUserManagement userManager)
        {
            _documentManager = documentManager;
            _userManager = userManager;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        }

        //[Authorize]
        //[HttpPost]
        //public async Task<IActionResult> UploadDocument(IFormCollection uploadFileForm)
        //{
        //    var userInfoRes = await _userManager.GetUserInfoAsync(HttpContext);

        //    var uploadFileModel = new UploadFileModel();
        //    uploadFileModel.Title = uploadFileForm["Title"]!;
        //    uploadFileModel.Description = uploadFileForm["Description"]!;
        //    uploadFileModel.Category = uploadFileForm["Category"]!;
        //    uploadFileModel.FileUpload = uploadFileForm.Files.FirstOrDefault()!;
        //    uploadFileModel.Version = uploadFileForm["Version"]!;

        //    var uploadDocumentRes = await _documentManager.UploadDocumentAsync(uploadFileModel, userInfoRes.Response!.Id);
        //    return StatusCode(201, new { uploadDocumentRes.Message });
        //}
        [Authorize]
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadDocument([FromForm] UploadFileModel uploadFileModel, [FromForm] int analyzeResponseId)
        {
            var userInfoRes = await _userManager.GetUserInfoAsync(HttpContext);

            var uploadDocumentRes = await _documentManager.UploadDocumentAsync(uploadFileModel, userInfoRes.Response!.Id, analyzeResponseId);
            return StatusCode(201, new { uploadDocumentRes.Message });
        }
        [Authorize]
        [HttpDelete("{documentId}")]
        public async Task<IActionResult> DeleteDocument(int documentId)
        {
            var userInfoRes = await _userManager.GetUserInfoAsync(HttpContext);
            var document = await _documentManager.DeleteDocumentAsync(documentId, userInfoRes.Response!.Id);
            if (!document.IsSuccess)
            {
                return StatusCode(document.StatusCode, new { document.Message });
            }
            return StatusCode(document.StatusCode, new { document.Message });
        }

        [Authorize]
        [HttpPatch("{documentId}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateDocument([FromForm] UpdateFileModel updateFileModel, int documentId, [FromForm] int analyzeResponseId)
        {
            var userInfoRes = await _userManager.GetUserInfoAsync(HttpContext);

            var updateFileRes = await _documentManager.UpdateDocumentAsync(updateFileModel, documentId, userInfoRes.Response!.Id, analyzeResponseId);
            return StatusCode(201, new { updateFileRes.Message });
        }

        [HttpGet("category")]
        public async Task<IActionResult> GetDocumentWithCategory(int categoryId)
        {
            var documents = await _documentManager.GetDocumentWithCategoryAsync(categoryId);
            if (!documents.IsSuccess)
            {
                return StatusCode(documents.StatusCode, new { documents.Message });
            }
            return StatusCode(documents.StatusCode, new { documents.Response });
        }

        [HttpGet]
        public async Task<IActionResult> GetDocuments()
        {
            var documents = await _documentManager.GetDocumentsAsync();
            if (!documents.IsSuccess)
            {
                return StatusCode(documents.StatusCode, new { documents.Message });
            }
            return StatusCode(documents.StatusCode, new { documents.Response });
        }

        [Authorize]
        [HttpGet("user")]
        public async Task<IActionResult> GetDocumentsFromUser()
        {
            var userInfoRes = await _userManager.GetUserInfoAsync(HttpContext);
            var documents = await _documentManager.GetUserDocumentsAsync(userInfoRes.Response!.Id);
            if (!documents.IsSuccess)
            {
                return StatusCode(documents.StatusCode, new { documents.Message });
            }
            return StatusCode(documents.StatusCode, new { documents.Response });
        }

        [HttpGet("{documentId}")]
        public async Task<IActionResult> GetDocument(int documentId)
        {
            var userInfoRes = await _userManager.GetUserInfoAsync(HttpContext);
            var document = await _documentManager.GetDocumentAsync(documentId, userInfoRes.Response!.Id);
            if (!document.IsSuccess)
            {
                return StatusCode(document.StatusCode, new { document.Message });
            }
            return StatusCode(document.StatusCode, new { document.Response });
        }

        [HttpGet("{documentId}/download")]
        public async Task<IActionResult> GetDocumentUrl(int documentId)
        {
            var url = await _documentManager.GetDocumentUrlAsync(documentId);
            if (!url.IsSuccess)
            {
                return StatusCode(url.StatusCode, new { url.Message });
            }
            return StatusCode(url.StatusCode, new { url.Response });
        }

        [HttpGet("get-categories")]
        public async Task<IActionResult> GetCategories()
        {
            var response = await _documentManager.GetCategoriesAsync();
            if (!response.IsSuccess)
            {
                return StatusCode(response.StatusCode, new { response.Message });
            }

            return StatusCode(response.StatusCode, new { response.Response });
        }

        [HttpGet("bookmarked")]
        public async Task<IActionResult> GetBookmarkedDocuments()
        {
            var userInfoRes = await _userManager.GetUserInfoAsync(HttpContext);
            var documents = await _documentManager.GetBookmarkedDocuments(userInfoRes.Response!.Id);
            if (!documents.IsSuccess)
            {
                return StatusCode(documents.StatusCode, new { documents.Message });
            }
            return StatusCode(documents.StatusCode, new { documents.Response });
        }

        [HttpPost("bookmark/{documentId}")]
        public async Task<IActionResult> CreateBookmarkDocument(int documentId)
        {
            var userInfoRes = await _userManager.GetUserInfoAsync(HttpContext);
            var document = await _documentManager.CreateBookmarkDocumentAsync(documentId, userInfoRes.Response!.Id);
            if (!document.IsSuccess)
            {
                return StatusCode(document.StatusCode, new { document.Message });
            }
            return StatusCode(document.StatusCode, new { document.Response });
        }
        [HttpDelete("bookmark/{documentId}")]
        public async Task<IActionResult> DeleteBookmarkDocument(int documentId)
        {
            var userInfoRes = await _userManager.GetUserInfoAsync(HttpContext);
            var document = await _documentManager.DeleteBookmarkDocumentAsync(documentId, userInfoRes.Response!.Id);
            if (!document.IsSuccess)
            {
                return StatusCode(document.StatusCode, new { document.Message });
            }
            return StatusCode(document.StatusCode, new { document.Message });
        }
    }
}
