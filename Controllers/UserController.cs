using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Application.Models;
using WebApplication1.Application.Services;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserManagement _userManagement;
        public UserController(IUserManagement userManagement)
        {
            _userManagement = userManagement;
        }

        [HttpGet("get-users")]
        public async Task<IActionResult> GetUsers()
        {
            var response = await _userManagement.GetListUser();
            if (response.IsSuccess)
            {
                return StatusCode(response.StatusCode, new { response = response.Response });
            }
            return StatusCode(response.StatusCode, new { message = response.Message });
        }

        [HttpGet]
        public async Task<IActionResult> GetUser()
        {
            var response = await _userManagement.GetUserInfoAsync(HttpContext);
            if (response.IsSuccess)
            {
                return StatusCode(response.StatusCode, new { response = response.Response });
            }
            return StatusCode(response.StatusCode, new { message = response.Message });
        }
    }
}
