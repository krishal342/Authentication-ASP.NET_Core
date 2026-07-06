using Authentication.Dtos;
using Authentication.Dtos.PasswordDtos;
using Authentication.Dtos.UserDtos;
using Authentication.Migrations;
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
        OtpService otpService,
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
            var unverifiedUser = await usersService.RegisterUserAsync(createUserDto);

            var otp = await otpService.GenerateOtpAsync();
            var verificationToken = await tokenService.GenerateRefreshTokenAsync();

            await otpService.StoreOtpAsync(unverifiedUser.Id, otp, verificationToken);

            //await emailService.SendEmailAsync(unverifiedUser.Email, "OTP for email verification.", $"The OTP for your email verification is: {otp}");

            Response.Cookies.Append("verificationToken", verificationToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddMinutes(double.Parse(config["Token:ExpireMinutes"]!)),
                Path = "/api/auth"
            });

            return Ok(new
            {
                message = "User registered successfully. Please check your email for the OTP to verify your account.",
                email = unverifiedUser.Email
            });

        }

        // verify user email
        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail(OtpDto otpDto)
        {
            var verificationToken = Request.Cookies["verificationToken"];

            if (string.IsNullOrEmpty(verificationToken))
            {
                throw new UnauthorizedAccessException("Expired or Invalid verification token.");
            }

            // return unverified user id if OTP and token is valid
            int unverifiedUserId = await otpService.VerifyOtpAsync(verificationToken, otpDto.Otp);  

            var user = await usersService.CreateUserAsync(unverifiedUserId);

            return CreatedAtAction(nameof(Login), new { id = user.Id }, new
            {
                message = "Email verified successfully. You can now log in.",
                email = user.Email
            });

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
            if (userIdClaim is null || !int.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("User ID not found in token.");
            }

            Response.Cookies.Delete("refreshToken");

            await tokenService.DeleteRefreshTokenAsync(userId);

            return Ok(new
            {
                token = ""
            });
        }

        #endregion

        #region reset password

        // reset password request
        [HttpPost("forget-password")]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordDto forgetPasswordDto)
        {

            var user = await usersService.GetUserByEmailAsync(forgetPasswordDto.Email);

            if (user is null)
            {
                return Ok();
            }

            var otp = await otpService.GenerateOtpAsync();
            var otpToken = await tokenService.GenerateRefreshTokenAsync();

            await otpService.StoreOtpAsync(user.Id, otp, otpToken);

            //await emailService.SendEmailAsync(user.Email, "OTP", $"The OTP for your password reset request is: {otp}");


            Response.Cookies.Append("otpToken", otpToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddMinutes(double.Parse(config["Token:ExpireMinutes"]!)),
                Path = "api/auth"
            });

            return Ok(new { message = "OTP sent successfully." });

        }


        // verify OTP
        [HttpPost("verify-otp")]
        public async Task<IActionResult> ValidateOTP(OtpDto otpDto)
        {
            var otpToken = Request.Cookies["otpToken"];

            if (string.IsNullOrEmpty(otpToken))
            {
                throw new UnauthorizedAccessException("Expired or Invalid OTP token.");
            }


            await otpService.VerifyOtpAsync(otpToken, otpDto.Otp);


            return Ok();

        }


        // reset password
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto resetPasswordDto)
        {
            var otpToken = Request.Cookies["otpToken"];

            if (string.IsNullOrEmpty(otpToken))
            {
                throw new UnauthorizedAccessException("Expired or Invalid OTP token.");
            }

            var userId = await otpService.VerifyOtpTokenAsync(otpToken);

            await usersService.ResetPasswordAsync(userId, resetPasswordDto);

            await otpService.DeleteOtpAsync(otpToken);

            return Ok(new { message = "Password reset successfully." });
        }

        #endregion


    }
}
