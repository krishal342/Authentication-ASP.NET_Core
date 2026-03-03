

using Authentication.Dtos.PasswordDtos;
using Authentication.Dtos.UserDtos;
using Authentication.Models;
using Authentication.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Authentication.Controllers
{
    [Authorize]
    [ApiController]
    [Route("/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UsersService _usersService;

        public UsersController(UsersService usersService)
        {
            _usersService = usersService;
        }

        // read
        [HttpGet]
        public async Task<IActionResult> ReadUsers()
        {
            var users = await _usersService.ReadUsersAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ReadUser(int id)
        {
            var user = await _usersService.ReadUserAsync(id);
            if (user == null)
                return NotFound();

            var responseUser = new ResponseUserDto
            {
                Id = id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                CreatedAt = user.CreatedAt
            };

            return Ok(responseUser);
        }

        // update
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UpdateUserDto updateUserDto)
        {
            try
            {
                var updatedUser = await _usersService.UpdateUserAsync(id, updateUserDto);
                return Ok(updatedUser);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
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
            catch (UnauthorizedAccessException)
            {
                var problem = new ProblemDetails
                {
                    Type = "https://httpstatuses.com/401",
                    Title = "Unauthorized",
                    Detail = "Password is incorrect.",
                    Status = StatusCodes.Status401Unauthorized
                };
                return Unauthorized(problem);
            }

        }

        [HttpPatch("{id}/change-password")]
        public async Task<IActionResult> ChangePassword(int id, ChangePasswordDto changePasswordDto)
        {
            try
            {
                await _usersService.ChangePasswordAsync(id, changePasswordDto);
                return NoContent();

            }
            catch (KeyNotFoundException)
            {

                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                var problem = new ProblemDetails
                {
                    Type = "https://httpstatuses.com/401",
                    Title = "Unauthorized",
                    Detail = "Current password is incorrect.",
                    Status = StatusCodes.Status401Unauthorized
                };
                return Unauthorized(problem);
            }
        }

        // delete
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id, DeleteUserDto deleteUserDto)
        {
            try
            {
                await _usersService.DeleteUserAsync(id, deleteUserDto);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                var problem = new ProblemDetails
                {
                    Type = "https://httpstatuses.com/401",
                    Title = "Unauthorized",
                    Detail = "Password is incorrect.",
                    Status = StatusCodes.Status401Unauthorized
                };
                return Unauthorized(problem);
            }
        }
    }
}