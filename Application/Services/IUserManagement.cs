using WebApplication1.Application.Models;
using WebApplication1.Domain.Entities;

namespace WebApplication1.Application.Services
{
    public interface IUserManagement
    {
        public Task<ApiResponse<User>> CreateUserWithTokenAsync(SignUpModel signUpModel);
        public Task<ApiResponse<User>> AssignRole(User user);
        public Task<ApiResponse<AuthResponse>> GetJwtTokenAsync(User user);
        public Task<ApiResponse<AuthResponse>> GetOtpByLoginAsync(SignInModel signInModel);
        public Task<ApiResponse<UserInfoModel>> GetUserInfoAsync(HttpContext httpContext);
        public Task<ApiResponse<User>> GetUserByEmailAsync(string email);
        public Task<ApiResponse<User>> GetUserAsync(HttpContext httpContext);
        public Task<ApiResponse<User>> GetUserIAsync(HttpContext httpContext);
        public Task<ApiResponse<List<UserInfoModel>>> GetListUser();
        public Task<ApiResponse<AuthResponse>> RenewAccessTokenAsync(AuthResponse authReponse);
        public Task<ApiResponse<string>> ChangeNewPasswordAsync(User user, ChangePasswordModel changePasswordModel);

    }
}
