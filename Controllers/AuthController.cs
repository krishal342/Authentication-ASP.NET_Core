using Authentication.Dtos.UserDtos;
using Authentication.Models;
using Authentication.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Authentication.Controllers
{
    [ApiController]
    [Route("/[controller]")]
    public class AuthController(TokenService tokenService, UsersService usersService, RefreshTokenService refreshTokenService, IConfiguration config) : ControllerBase
    {

        // register user
        [HttpPost("register")]
        public async Task<IActionResult> Register(CreateUserDto createUserDto)
        {
            try
            {
                var createdUser = await usersService.CreateUserAsync(createUserDto);
                return Ok(createdUser);
            }
            catch (InvalidOperationException)
            {
                var problem = new ProblemDetails
                {
                    Type = "https://httpstatuses.com/409",
                    Title = "Conflict",
                    Detail = "Email already exits.",
                    Status = StatusCodes.Status409Conflict
                };
                return Conflict(problem);
            }
        }


        // login user 
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginUserDto userDto)
        {
            var user = await usersService.GetUserByEmailAsync(userDto.Email);

            if (user is null)
            {
                var problem = new ProblemDetails
                {
                    Type = "https://httpstatuses.com/400",
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Invalid email or password",
                    Detail = "Invalid email or password"
                };
                return BadRequest(problem);
            }


            var hasher = new PasswordHasher<User>();

            var verifyResult = hasher.VerifyHashedPassword(user, user.Password, userDto.Password);

            if (verifyResult == PasswordVerificationResult.Failed)
            {
                var problem = new ProblemDetails
                {
                    Type = "https://httpstatuses.com/400",
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Invalid email or password",
                    Detail = "Invalid email or password"
                };
                return BadRequest(problem);
            }

            var accessToken = await tokenService.GenerateAccessTokenAsync(user.Id.ToString(), user.Email);

            var refreshToken = await tokenService.GenerateRefreshTokenAsync();


            await refreshTokenService.CreateAsync(user.Id, refreshToken);


            Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(double.Parse(config["RefreshToken:ExpireDays"]!)),
                Path = "/auth/refresh"
            });


            var responseUser = new ResponseUserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                CreatedAt = user.CreatedAt
            };

            return Ok(new { user = responseUser, token = accessToken });

        }


        // refresh access token
        [HttpGet("refresh")]
        public async Task<IActionResult> RefreshAccessToken()
        {
            try
            {

                var refreshToken = Request.Cookies["refreshToken"];
                if (string.IsNullOrEmpty(refreshToken))
                {
                    var problem = new ProblemDetails
                    {
                        Type = "https://httpstatuses.com/401",
                        Title = "Unauthorized",
                        Detail = "Expired or Invalid refresh token.",
                        Status = StatusCodes.Status401Unauthorized
                    };
                    return Unauthorized(problem);
                }

                var userId = await refreshTokenService.ValidateAsync(refreshToken);

                var user = await usersService.GetUserAsync(userId);

                var accessToken = await tokenService.GenerateAccessTokenAsync(user!.Id.ToString(), user.Email);

                var responseUser = new ResponseUserDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    CreatedAt = user.CreatedAt
                };

                return Ok(new { user = responseUser, token = accessToken });
            }
            catch (KeyNotFoundException)
            {
                var problem = new ProblemDetails
                {
                    Type = "https://httpstatuses.com/401",
                    Title = "Unauthorized",
                    Detail = "Expired or Invalid refresh token.",
                    Status = StatusCodes.Status401Unauthorized
                };
                return Unauthorized(problem);
            }
        }


        // reset password request
        [HttpPost("/forget-password")]
        public async Task<IActionResult> ForgetPassword()
        {
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



    }
}
