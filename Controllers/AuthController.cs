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

        public AuthController(TokenService tokenService,UsersService usersService)
        {
            _tokenService = tokenService;

            _usersService = usersService;

        }

        // login user and return token  
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
            var token = _tokenService.GenerateToken(user.Id.ToString(), user.Email);

            Response.Cookies.Append("token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7)
            });

            var responseUser = new ResponseUserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                CreatedAt = user.CreatedAt
            };

            return Ok(responseUser);



        }
    }
}
