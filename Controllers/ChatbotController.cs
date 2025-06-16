using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using WebApplication1.Application.Models;
using WebApplication1.Application.Services;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class ChatbotController : ControllerBase
    {
        private readonly IChatbotService _chatbotService;
        private readonly IUserManagement _userManager;

        public ChatbotController(IChatbotService chatbotService, IUserManagement userManager)
        {
            _chatbotService = chatbotService;
            _userManager = userManager;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        }
        [Authorize]
        [HttpPost("chat")]
        public async Task<IActionResult> RequestChat([FromBody] ChatRequest request)
        {
            var userInfoRes = await _userManager.GetUserInfoAsync(HttpContext);

            var chatResponse = await _chatbotService.GetChatResponseAsync(userInfoRes.Response!.Id, request.question);

            return StatusCode(201, new { chatResponse.Response });
        }

        [HttpPost("analyze")]
        public async Task<IActionResult> AnalyzeDocument(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }
            var response = await _chatbotService.AnalyzeDocumentAsync(file);
            if (response.IsSuccess)
            {
                return Ok(response.Response);
            }
            return StatusCode(response.StatusCode, new { message = response.Message });
        }

    }
}
