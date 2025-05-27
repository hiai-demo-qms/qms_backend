using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Application.Models;
using WebApplication1.Application.Services;

namespace WebApplication1.Controllers
{
    public class UserController : ControllerBase
    {
        private readonly IUserManagement _userManagement;
        public UserController(IUserManagement userManagement)
        {
            _userManagement = userManagement;
        }

        //[HttpPost("sign-up")]
        //public async Task<IActionResult> SignUp([FromBody] SignUpModel signUpModel)
        //{
        //    if (signUpModel == null)
        //    {
        //        return BadRequest("Sign-up model cannot be null.");
        //    }
        //    var resSignUp = await _userManagement.CreateUserWithTokenAsync(signUpModel);
        //    if (resSignUp.IsSuccess)
        //    {
        //        return StatusCode(resSignUp.StatusCode, new {message = resSignUp.Message});
        //    }
        //    var resAssignRole = await _userManagement.AssignRole(resSignUp.Response!);
        //    if (resAssignRole.IsSuccess)
        //    {
        //        return StatusCode(resAssignRole.StatusCode, new {message = resAssignRole.Message});
        //    }
        //    return StatusCode(resSignUp.StatusCode, new { message = resSignUp.Message });
        //}

        [HttpGet("get-users")]
        public async Task<IActionResult> GetUsers()
        {
            var response = await _userManagement.GetListUser();
            if (response.IsSuccess)
            {
                return Ok(response.Response);
            }
            return StatusCode(response.StatusCode, new { message = response.Message });
        }
    }
}
