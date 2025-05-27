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
        public async Task<IActionResult> UploadDocument([FromForm] UploadFileModel uploadFileModel)
        {
            var userInfoRes = await _userManager.GetUserInfoAsync(HttpContext);

            var uploadDocumentRes = await _documentManager.UploadDocumentAsync(uploadFileModel, userInfoRes.Response!.Id);
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
        public async Task<IActionResult> EditProduct([FromForm] UploadFileModel updateFileModel, int documentId)
        {
            var userInfoRes = await _userManager.GetUserInfoAsync(HttpContext);

            var updateFileRes = await _documentManager.UpdateDocumentAsync(updateFileModel, documentId, userInfoRes.Response!.Id);
            return StatusCode(201, new { updateFileRes.Message });
        }

        [HttpGet("category/{documentId}")]
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
            var userInfoRes = await _userManager.GetUserInfoAsync(HttpContext);
            var url = await _documentManager.GetDocumentUrlAsync(documentId, userInfoRes.Response!.Id);
            if (!url.IsSuccess)
            {
                return StatusCode(url.StatusCode, new { url.Message });
            }
            return StatusCode(url.StatusCode, new { url.Response });
        }
    }
}
