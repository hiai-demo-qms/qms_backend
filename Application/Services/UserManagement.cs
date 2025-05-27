using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WebApplication1.Application.Models;
using WebApplication1.Domain.Entities;
using WebApplication1.Infrastructure.Data;

namespace WebApplication1.Application.Services
{
    public class UserManagement : IUserManagement
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly QmsDbContext _dbcontext;


        public UserManagement(UserManager<User> userManager, SignInManager<User> signInManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration, QmsDbContext dbcontext)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _dbcontext = dbcontext;
        }
        public async Task<ApiResponse<User>> AssignRole(User user)
        {
            if (user == null)
            {
                return new ApiResponse<User>
                {
                    IsSuccess = false,
                    Message = "User is null",
                    StatusCode = 400,
                };
            }

            var role = "Admin";

            var userCount = await _userManager.Users.CountAsync();

            if (userCount > 1)
            {
                role = "User";
            }

            if (!await _roleManager.RoleExistsAsync(role))
            {
                return new ApiResponse<User>
                {
                    IsSuccess = false,
                    Message = "Role is not exist",
                    StatusCode = 400,
                };
            }

            var rs = await _userManager.AddToRoleAsync(user, role);
            if (!rs.Succeeded)
            {
                return new ApiResponse<User>
                {
                    IsSuccess = false,
                    Message = "Couldn't create role for the user",
                    StatusCode = 400,
                };
            }

            return new ApiResponse<User>
            {
                IsSuccess = true,
                Message = "Create role successfully",
                StatusCode = 201,
            };
        }

        public Task<ApiResponse<string>> ChangePasswordAsync(User user, ChangePasswordModel changePasswordModel)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<User>> CreateUserWithTokenAsync(SignUpModel signUpModel)
        {
            var isEmailExists = await _userManager.FindByEmailAsync(signUpModel.Email!);
            if (isEmailExists != null)
            {
                return new ApiResponse<User>
                {
                    IsSuccess = false,
                    Message = "Email is already in use",
                    StatusCode = 400,
                };
            }

            User user = new User()
            {
                Email = signUpModel.Email,
                FullName = signUpModel.FullName,
                UserName = signUpModel.Email,
                EmailConfirmed = true

            };
            var result = await _userManager.CreateAsync(user, signUpModel.Password!);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                return new ApiResponse<User>
                {
                    IsSuccess = false,
                    Message = $"Create account failed: {errors}",
                    StatusCode = 400
                };

            }
            return new ApiResponse<User>
            {
                IsSuccess = true,
                Message = "Create account successfully",
                StatusCode = 201,
                Response = user
            };

        }

        public async Task<ApiResponse<List<UserInfoModel>>> GetListUser()
        {
            var users = await _dbcontext.Users.ToListAsync();
            if (users == null || !users.Any())
            {
                return new ApiResponse<List<UserInfoModel>>
                {
                    IsSuccess = false,
                    Message = "No users found",
                    StatusCode = 404
                };
            }
            List<UserInfoModel> userInfoModels = users.Select(user => new UserInfoModel
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                UserName = user.UserName,
                Role = _userManager.GetRolesAsync(user).Result.ToList()[0]
            }).ToList();

            if (userInfoModels == null || !userInfoModels.Any())
            {
                return new ApiResponse<List<UserInfoModel>>
                {
                    IsSuccess = false,
                    Message = "No user information found",
                    StatusCode = 404
                };
            }

            return new ApiResponse<List<UserInfoModel>>
            {
                IsSuccess = true,
                Message = "Users retrieved successfully",
                StatusCode = 200,
                Response = userInfoModels
            };
        }

        public async Task<ApiResponse<AuthResponse>> GetJwtTokenAsync(User user)
        {
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                authClaims.Add(new(ClaimTypes.Role, role));
            }
            var jwtToken = GetToken(authClaims);
            var refreshToken = GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiration = DateTime.UtcNow.AddDays(1);
            await _userManager.UpdateAsync(user);
            return new ApiResponse<AuthResponse>
            {
                IsSuccess = true,
                Message = "Token is created successfully",
                StatusCode = 201,
                Response = new AuthResponse
                {
                    AccessToken = new TokenType()
                    {
                        Token = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                        ExpirationTokenDate = jwtToken.ValidTo
                    },
                    RefreshToken = new TokenType()
                    {
                        Token = user.RefreshToken,
                        ExpirationTokenDate = (DateTime)user.RefreshTokenExpiration,
                    }

                }
            };
        }

        public async Task<ApiResponse<AuthResponse>> GetOtpByLoginAsync(SignInModel signInModel)
        {
            var user = await _userManager.FindByEmailAsync(signInModel.Email);
            if (user == null)
            {
                return new ApiResponse<AuthResponse> { IsSuccess = false, StatusCode = 404, Message = "The user with that email isn't existed" };
            }
            if (!await _userManager.CheckPasswordAsync(user, signInModel.Password))
            {
                return new ApiResponse<AuthResponse> { IsSuccess = false, StatusCode = 404, Message = "Password is incorrect" };
            }
            await _signInManager.SignOutAsync();
            await _signInManager.PasswordSignInAsync(user, signInModel.Password, false, true);

            return await GetJwtTokenAsync(user);

        }

        public Task<ApiResponse<User>> GetUserAsync(HttpContext httpContext)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<User>> GetUserByEmailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                return new ApiResponse<User> { IsSuccess = true, StatusCode = 200, Message = "Get user successfully", Response = user };
            }

            return new ApiResponse<User> { IsSuccess = false, StatusCode = 404, Message = "No user found" };
        }

        public async Task<ApiResponse<UserInfoModel>> GetUserInfoAsync(HttpContext httpContext)
        {
            User? user = await _userManager.FindByNameAsync(httpContext.User.FindFirstValue(ClaimTypes.Name));

            if (user == null)
            {
                return new ApiResponse<UserInfoModel> { IsSuccess = false, StatusCode = 401, Message = "Coudn't found the user from the token" };
            }
            return new ApiResponse<UserInfoModel>
            {
                IsSuccess = true,
                StatusCode = 200,
                Message = "The user is found",
                Response = new UserInfoModel()
                {
                    Id = user.Id,
                    Email = user.Email!,
                    UserName = user.UserName!,
                    Role = (await _userManager.GetRolesAsync(user))[0],
                }
            };
        }

        public async Task<ApiResponse<AuthResponse>> RenewAccessTokenAsync(AuthResponse authReponse)
        {
            var accessToken = authReponse.AccessToken;
            var refreshToken = authReponse.RefreshToken;
            var principal = GetClaimsPrincipal(accessToken.Token);
            var user = await _userManager.FindByEmailAsync(principal.Identity.Name);
            if (user != null && refreshToken.Token == user.RefreshToken && user.RefreshTokenExpiration > DateTime.Now)
            {
                var resposne = await GetJwtTokenAsync(user);
                return resposne;
            }
            return new ApiResponse<AuthResponse> { IsSuccess = false, StatusCode = 400, Message = "Refresh Token is expired" };
        }

        public async Task<ApiResponse<string>> ChangeNewPasswordAsync(User user, ChangePasswordModel changePasswordModel)
        {
            var rs = await _userManager.ChangePasswordAsync(user, changePasswordModel.OldPassword, changePasswordModel.NewPassword);
            if (!rs.Succeeded)
            {
                return new ApiResponse<string> { IsSuccess = false, StatusCode = 400, Message = "The current password is incorrect" };
            }
            return new ApiResponse<string> { IsSuccess = true, StatusCode = 200, Message = "Password is changed successfully" };
        }

        #region PrivateMethods
        private JwtSecurityToken GetToken(List<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]!));
            var token = new JwtSecurityToken(
                issuer: _configuration["ValidIssuer"],
                audience: _configuration["ValidAudience"],
                expires: DateTime.Now.AddMinutes(45),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );
            return token;
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new Byte[64];
            var range = RandomNumberGenerator.Create();
            range.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private ClaimsPrincipal GetClaimsPrincipal(string accessToken)
        {
            var tokenValidationParams = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]))
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(accessToken, tokenValidationParams, out SecurityToken securityToken);
            return principal;
        }
        #endregion
    }
}
