using Microsoft.EntityFrameworkCore;
using Authentication.Data;
using Authentication.Models;
using Authentication.Dtos.UserDtos;
using Microsoft.AspNetCore.Identity;
using Authentication.Dtos.PasswordDtos;
using Authentication.Exceptions;

namespace Authentication.Services
{


    public class UsersService(ApplicationDbContext context, IPasswordHasher<User> passwordHasher)
    {

        private readonly ApplicationDbContext _context = context ?? throw new ArgumentNullException(nameof(context));
        private readonly IPasswordHasher<User> _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));

        #region Create Operation

        // create user 
        public async Task<ResponseUserDto> CreateUserAsync(CreateUserDto createUserDto)
        {

            var email = createUserDto.Email.ToLower().Trim();

            var existingUser = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email);

            if (existingUser != null)
            {
                throw new EmailAlreadyExistsException();
            }

            var newUser = new User
            {
                FirstName = createUserDto.FirstName.Trim(),
                LastName = createUserDto.LastName.Trim(),
                Email = email,
                Password = _passwordHasher.HashPassword(null!, createUserDto.Password),
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

        #endregion

        #region Read Operations

        // get all user
        public async Task<List<User>> GetUsersAsync(int pageNumber = 1, int pageSize = 10) =>
            await _context.Users
            .AsNoTracking()
            .OrderBy(u => u.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // get user by id
        public async Task<User> GetUserAsync(int id)
        {
            var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);

            if(user is null)
            {
                throw new KeyNotFoundException("User not found.");
            }
            return user;
        }
            

        // get user by email
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            var normalizedEmail = email.ToLower().Trim();

            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync (u => u.Email == normalizedEmail);
        }

        // check if email exists without loading full user objects
        public async Task<bool> EmailExistsAsync(string email)
        {
            var normalizedEmail = email.ToLower().Trim();

            return await _context.Users
                .AsNoTracking()
                .AnyAsync(u => u.Email == normalizedEmail);
        }

        #endregion

        #region Update Operations

        // update user detail
        public async Task<ResponseUserDto> UpdateUserAsync(int id, UpdateUserDto updateUserDto)
        {

            var user = await _context.Users.FindAsync(id);

            if (user is null)
            {
                throw new KeyNotFoundException("User not found.");
            }

            var verifyResult = _passwordHasher.VerifyHashedPassword(user, user.Password, updateUserDto.Password);

            if (verifyResult == PasswordVerificationResult.Failed)
            {
                throw new IncorrectPasswordException();
            }

            var normalizeEmail = updateUserDto.Email?.ToLower().Trim();
            var emailExists = await _context.Users
                .AsNoTracking()
                .AnyAsync (u => u.Email == normalizeEmail);

            if (emailExists)
            {
                throw new EmailAlreadyExistsException();
            }

            user.FirstName = updateUserDto.FirstName?.Trim() ?? user.FirstName;
            user.LastName = updateUserDto.LastName?.Trim() ?? user.LastName;
            user.Email = normalizeEmail ?? user.Email;

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
                throw new KeyNotFoundException("User not found.");
            }
            var verifyResult = _passwordHasher.VerifyHashedPassword(user, user.Password, changePasswordDto.CurrentPassword);
            if (verifyResult == PasswordVerificationResult.Failed)
            {
                throw new IncorrectPasswordException();
            }
            user.Password = _passwordHasher.HashPassword(user, changePasswordDto.NewPassword);
            await _context.SaveChangesAsync();
        }

        #endregion

        // reset password

        #region Delete Operation

        // hard delete user
        public async Task DeleteUserAsync(int id, DeleteUserDto deleteUserDto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user is null)
            {
                throw new KeyNotFoundException("User not found.");
            }

            var verifyResult = _passwordHasher.VerifyHashedPassword(user, user.Password, deleteUserDto.Password);

            if (verifyResult == PasswordVerificationResult.Failed)
            {
                throw new IncorrectPasswordException();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }

        #endregion

        #region Login Operation

        // login
        public async Task<User> LoginAsync(string email, string password)
        {
            var normalizedEmail = email.ToLower().Trim();

            var user = _context.Users.FirstOrDefault(u => u.Email == normalizedEmail);
            if (user is null)
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            var verifyResut = _passwordHasher.VerifyHashedPassword(user,user.Password, password);

            if(verifyResut == PasswordVerificationResult.Failed)
            {
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            return user;
        }

        #endregion
    }
}
