using Authentication.Dtos.UserDtos;
using Authentication.Models;
using Authentication.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Authentication.Controllers
{
    [ApiController]
    [Route("/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly TokenService _tokenService;
        private readonly UsersService _usersService;
        private readonly RefreshTokenService _refreshTokenService;
        private readonly IConfiguration _config;


        public AuthController(TokenService tokenService, UsersService usersService, RefreshTokenService refreshTokenService, IConfiguration config)
        {
            _tokenService = tokenService;

            _usersService = usersService;

            _refreshTokenService = refreshTokenService;

            _config = config;
        }

        // register user
        [HttpPost("register")]
        public async Task<IActionResult> Register(CreateUserDto createUserDto)
        {
            try
            {
                var createdUser = await _usersService.CreateUserAsync(createUserDto);
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

        // login user and set token as cookie
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginUserDto userDto)
        {
            var user = await _usersService.ReadUserByEmailAsync(userDto.Email);
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

            var accessToken = await _tokenService.GenerateAccessTokenAsync(user.Id.ToString(), user.Email);

            var refreshToken = await _tokenService.GenerateRefreshTokenAsync();

            await _refreshTokenService.CreateRefreshTokenAsync(user.Id, refreshToken);

            Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddMinutes(double.Parse(_config["RefreshToken:ExpireMinutes"]!)),
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

                var userId = await _refreshTokenService.ValidateAsync(refreshToken);

                var user = await _usersService.ReadUserAsync(userId);

                var accessToken = await _tokenService.GenerateAccessTokenAsync(user!.Id.ToString(), user.Email);

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

        // logout
        //[HttpGet("logout")]
        //public async Task<IActionResult> Logout()
        //{

        //}



    }
}
