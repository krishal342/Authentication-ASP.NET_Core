using Authentication.Dtos.PasswordDtos;
using Authentication.Dtos.UserDtos;
using Authentication.Models;
using Authentication.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Authentication.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(
        TokenService tokenService, 
        UsersService usersService, 
        EmailService emailService, 
        IConfiguration config) : ControllerBase
    {

        // helper function to send response after login or refresh access token
        private async Task<IActionResult> LoginResponse(User user, string accessToken)
        {
            var responseUser = new ResponseUserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                CreatedAt = user.CreatedAt
            };

            return Ok(new
            {
                user = responseUser,
                token = accessToken
            });
        }

        #region register user

        // register user
        [HttpPost("register")]
        public async Task<IActionResult> Register(CreateUserDto createUserDto)
        {

            var createdUser = await usersService.CreateUserAsync(createUserDto);
            return CreatedAtAction(nameof(Login), new { email = createdUser.Email }, createdUser);

        }
        #endregion

        #region login user

        // login user 
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginUserDto userDto)
        {
            var user = await usersService.LoginAsync(userDto.Email, userDto.Password);

            var accessToken = await tokenService.GenerateAccessTokenAsync(user.Id.ToString(), user.Email);

            var refreshToken = await tokenService.GenerateRefreshTokenAsync();


            await tokenService.CreateAsync(user.Id, refreshToken);


            Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(double.Parse(config["RefreshToken:ExpireDays"]!)),
                Path = "api/auth/refresh"
            });

            return await LoginResponse(user, accessToken);


        }
        #endregion

        #region refresh access token

        // refresh access token
        [HttpGet("refresh")]
        public async Task<IActionResult> RefreshAccessToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
                throw new UnauthorizedAccessException("Expired or Invalid refresh token.");

            var userId = await tokenService.ValidateAsync(refreshToken);

            var user = await usersService.GetUserAsync(userId);

            var accessToken = await tokenService.GenerateAccessTokenAsync(user.Id.ToString(), user.Email);

            return await LoginResponse(user, accessToken);

        }
        #endregion

        #region logout user

        [Authorize]
        [HttpGet("logout")]
        public async Task<IActionResult> Logout()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if(userIdClaim is null || int.TryParse(userIdClaim,out var userId))
            {
                throw new UnauthorizedAccessException("User ID not found in token.");
            }

            Response.Cookies.Delete("refreshToken");

            await tokenService.DeleteRefreshTokenAsync(userId);

            return Ok(new {
                token = ""
            });
        }

        #endregion

        #region reset password
        // reset password request
        [HttpPost("/forget-password")]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordDto forgetPasswordDto)
        {
            var user = usersService.GetUserByEmailAsync(forgetPasswordDto.Email);
            if (user is null)
            {
                return Ok();
            }

            await emailService.SendEmailAsync("krishal342@gmail.com", "OTP", "OTP Testing.");

            return Ok("forget password");
        }


        // validate OTP
        [HttpPost("/validate-otp")]
        public async Task<IActionResult> ValidateOTP()
        {
            return Ok("Validate OTP");
        }


        // reset password
        [HttpPost("/reset-password")]
        public async Task<IActionResult> ResetPassword()
        {
            return Ok("Reset Password");
        }

        #endregion


    }
}
