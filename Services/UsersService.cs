using Microsoft.EntityFrameworkCore;
using Authentication.Data;
using Authentication.Models;
using Microsoft.AspNetCore.Mvc;
using Authentication.Dtos.UserDtos;
using Microsoft.AspNetCore.Identity;
using Authentication.Dtos.PasswordDtos;

namespace Authentication.Services
{
    public class UsersService(ApplicationDbContext context)
    {
        private readonly ApplicationDbContext _context = context;

        // create
        public async Task<ResponseUserDto> CreateUserAsync(CreateUserDto createUserDto)
        {
            var email = createUserDto.Email.ToLower();

            if (_context.Users.Any(u => u.Email == email))
            {
                throw new InvalidOperationException();
            }

            var hasher = new PasswordHasher<User>();

            var newUser = new User
            {
                FirstName = createUserDto.FirstName,
                LastName = createUserDto.LastName,
                Email = email,
                Password = hasher.HashPassword(null!, createUserDto.Password),
            };

            await _context.Users.AddAsync(newUser);
            await _context.SaveChangesAsync();

            return new ResponseUserDto
            {
                Id = newUser.Id,
                FirstName = newUser.FirstName,
                LastName = newUser.LastName,
                Email = newUser.Email,
                CreatedAt = newUser.CreatedAt
            };


        }

        // read
        // get all user
        public async Task<List<User>> GetUsersAsync() =>
            await _context.Users.ToListAsync();

        // get user by id
        public async Task<User?> GetUserAsync(int id) =>
            await _context.Users.FindAsync(id);

        // get user by email
        public async Task<User?> GetUserByEmailAsync(string email) =>
            await _context.Users.FirstOrDefaultAsync(u => u.Email == email.ToLower());

        // update

        // update user detail
        public async Task<ResponseUserDto> UpdateUserAsync(int id, UpdateUserDto updateUserDto)
        {

            var user = await _context.Users.FindAsync(id);
            if (user is null)
            {
                throw new KeyNotFoundException();
            }

            var email = updateUserDto.Email?.ToLower();

            if (_context.Users.Any(u => u.Email == email))
            {
                throw new InvalidOperationException();
            }

            var hasher = new PasswordHasher<User>();

            var verifyResult = hasher.VerifyHashedPassword(user, user.Password, updateUserDto.Password);

            if (verifyResult == PasswordVerificationResult.Failed)
            {
                throw new UnauthorizedAccessException();
            }

            user.FirstName = updateUserDto.FirstName ?? user.FirstName;
            user.LastName = updateUserDto.LastName ?? user.LastName;
            user.Email = email ?? user.Email;

            await _context.SaveChangesAsync();

            return new ResponseUserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                CreatedAt = user.CreatedAt
            };

        }

        // change password
        public async Task ChangePasswordAsync(int id, ChangePasswordDto changePasswordDto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user is null)
            {
                throw new KeyNotFoundException();
            }
            var hasher = new PasswordHasher<User>();
            var verifyResult = hasher.VerifyHashedPassword(user, user.Password, changePasswordDto.CurrentPassword);
            if (verifyResult == PasswordVerificationResult.Failed)
            {
                throw new UnauthorizedAccessException();
            }
            user.Password = hasher.HashPassword(user, changePasswordDto.NewPassword);
            await _context.SaveChangesAsync();
        }

        // reset password

        // delete
        public async Task DeleteUserAsync(int id, DeleteUserDto deleteUserDto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user is null)
            {
                throw new KeyNotFoundException();
            }

            var hasher = new PasswordHasher<User>();
            var verifyResult = hasher.VerifyHashedPassword(user, user.Password, deleteUserDto.Password);

            if (verifyResult == PasswordVerificationResult.Failed)
            {
                throw new UnauthorizedAccessException();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }


    }
}
