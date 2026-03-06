using Authentication.Data;
using Authentication.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Authentication.Services
{
    public class TokenService(ApplicationDbContext context, IConfiguration config)
    {
        private readonly IConfiguration _config = config ?? throw new ArgumentNullException(nameof(config));
        private readonly ApplicationDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

        // generate access token
        public async Task<string> GenerateAccessTokenAsync(string userId, string email) 
        {

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub,userId),
                new Claim(JwtRegisteredClaimNames.Email,email),
                new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())
            };


            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(_config["Jwt:ExpireMinutes"]!)),
                signingCredentials: creds
            );

            var tokenString =  new JwtSecurityTokenHandler().WriteToken(token);


            return tokenString;
        }

        // generate refresh token
        public async Task<string> GenerateRefreshTokenAsync()
        {
            var randomBytes = new byte[64]; // 512-bit token
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);

        }

        // create refresh token record
        public async Task CreateAsync(int userId, string refreshToken)
        {
            var newRefreshToken = new RefreshToken
            {
                Token = refreshToken,
                UserId = userId,
                ExpiresAt = DateTime.UtcNow.AddDays(double.Parse(_config["RefreshToken:ExpireDays"]!)),
                CreatedAt = DateTime.UtcNow
            };

            await _context.RefreshTokens.AddAsync(newRefreshToken);
            await _context.SaveChangesAsync();

        }

        // validate refresh token
        public async Task<int> ValidateAsync(string refreshToken)
        {
            var tokenDetail = await _context.RefreshTokens.FirstOrDefaultAsync(x => x.Token == refreshToken);

            if (tokenDetail is null || tokenDetail.IsActive is false)
            {
                throw new KeyNotFoundException("Token not found.");
            }

            return tokenDetail.UserId;

        }

        // delete refresh token
        public async Task DeleteRefreshTokenAsync(int userId)
        {
            var refreshTokens = await _context.RefreshTokens
                .Where(t => t.UserId == userId)
                .ExecuteDeleteAsync();
        }

    }
}
