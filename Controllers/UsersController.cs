
using Authentication.Dtos.PasswordDtos;
using Authentication.Dtos.UserDtos;
using Authentication.Exceptions;
using Authentication.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Authentication.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController(UsersService usersService) : ControllerBase
    {

        // get userId from token, or throw exception
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                throw new UnauthorizedAccessException("User ID not found in token.");

            return userId;
        }

        // check if given id match logged in useId
        private void VerifyUser(int id)
        {
            var currentUserId = GetCurrentUserId();

            if(currentUserId != id)
            {
                throw new AccessDeniedException();
            }
        }

        // get all user (paginated)
        [HttpGet]
        public async Task<IActionResult> GetUsers(int pageNumber = 1, int pageSize = 10)
        {
            var users = await usersService.GetUsersAsync(pageNumber,pageSize);
            return Ok(new
            {
                pageNumber,
                pageSize,
                totalCount = users.Count,
                users
            });
        }

        // get current authenticated user's profile
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUserProfile()
        {
            var currentUserId = GetCurrentUserId();

            var user = await usersService.GetUserAsync(currentUserId);

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

        // get user by id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await usersService.GetUserAsync(id);

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


        // update logged in user detail
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UpdateUserDto updateUserDto)
        {
            VerifyUser(id);

            var updatedUser = await usersService.UpdateUserAsync(id, updateUserDto);
            return Ok(updatedUser);

        }


        // change password of logged in user
        [HttpPatch("{id}/change-password")]
        public async Task<IActionResult> ChangePassword(int id, ChangePasswordDto changePasswordDto)
        {
            VerifyUser(id);

            await usersService.ChangePasswordAsync(id, changePasswordDto);
            return NoContent();

        }


        // delete logged in user
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id, DeleteUserDto deleteUserDto)
        {
            VerifyUser(id);

            await usersService.DeleteUserAsync(id, deleteUserDto);
            return NoContent();

        }
    }
}