
using Authentication.Dtos.PasswordDtos;
using Authentication.Dtos.UserDtos;
using Authentication.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Authentication.Controllers
{
    [Authorize]
    [ApiController]
    [Route("/[controller]")]
    public class UsersController(UsersService usersService) : ControllerBase
    {

        // read all user
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await usersService.GetUsersAsync();
            return Ok(users);
        }

        // read user by id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await usersService.GetUserAsync(id);
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


        // update logged in user detail
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UpdateUserDto updateUserDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (int.Parse(userId!) != id)
                {
                    return Forbid();
                }

                var updatedUser = await usersService.UpdateUserAsync(id, updateUserDto);
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


        // change password of logged in user
        [HttpPatch("{id}/change-password")]
        public async Task<IActionResult> ChangePassword(int id, ChangePasswordDto changePasswordDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (int.Parse(userId!) != id)
                {
                    return Forbid();
                }
                await usersService.ChangePasswordAsync(id, changePasswordDto);
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


        // delete logged in user
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id, DeleteUserDto deleteUserDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (int.Parse(userId!) != id)
                {
                    return Forbid();
                }

                await usersService.DeleteUserAsync(id, deleteUserDto);
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