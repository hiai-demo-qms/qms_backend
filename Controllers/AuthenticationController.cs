using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Application.Models;
using WebApplication1.Application.Services;

namespace WebApplication1.Controllers
{
    public class AuthenticationController : ControllerBase
    {
        private readonly IUserManagement _userManagement;
        public AuthenticationController(IUserManagement userManagement)
        {
            _userManagement = userManagement;
        }

        [HttpPost("sign-up")]
        public async Task<IActionResult> SignUp([FromBody] SignUpModel signUpModel)
        {
            if (signUpModel == null)
            {
                return BadRequest("Sign-up model cannot be null.");
            }
            var resSignUp = await _userManagement.CreateUserWithTokenAsync(signUpModel);
            if (!resSignUp.IsSuccess)
            {
                return StatusCode(resSignUp.StatusCode, new { message = resSignUp.Message });
            }
            var resAssignRole = await _userManagement.AssignRole(resSignUp.Response!);
            if (!resAssignRole.IsSuccess)
            {
                return StatusCode(resAssignRole.StatusCode, new { message = resAssignRole.Message });
            }
            var resJwt = await _userManagement.GetJwtTokenAsync(resSignUp.Response!);
            if (!resJwt.IsSuccess)
            {
                return StatusCode(resJwt.StatusCode, new { message = resJwt.Message });
            }
            return StatusCode(resJwt.StatusCode, new { message = resJwt.Message, token = resJwt.Response });
        }

        [HttpPost("sign-in")]
        public async Task<IActionResult> SignIn([FromBody] SignInModel signInModel)
        {
            var resSignIn = await _userManagement.GetOtpByLoginAsync(signInModel);
            if (!resSignIn.IsSuccess)
            {
                return StatusCode(resSignIn.StatusCode, new { message = resSignIn.Message });
            }
            return StatusCode(resSignIn.StatusCode, new { message = resSignIn.Message, token = resSignIn.Response });
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] AuthResponse authReponse)
        {
            var jwt = await _userManagement.RenewAccessTokenAsync(authReponse);
            if (jwt.IsSuccess)
            {
                return Ok(jwt.Response);
            }
            return StatusCode(jwt.StatusCode, new { jwt.Message });

        }
        [HttpPost("change-password")]
        [Authorize]

        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel changePasswordModel)
        {
            var userInfoRes = await _userManagement.GetUserAsync(HttpContext);

            var changePassword = await _userManagement.ChangeNewPasswordAsync(userInfoRes.Response, changePasswordModel);
            return StatusCode(changePassword.StatusCode, new { changePassword.Message });

        }
    }
}
