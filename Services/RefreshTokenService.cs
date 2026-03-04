using Authentication.Data;
using Authentication.Models;
using Microsoft.EntityFrameworkCore;

namespace Authentication.Services
{
    public class RefreshTokenService(ApplicationDbContext context, IConfiguration config)
    {
        private readonly ApplicationDbContext _context = context;
        private readonly IConfiguration _config = config;

        // create
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
        // read
        public async Task<int> ValidateAsync(string refreshToken)
        {
            var tokenDetail = await _context.RefreshTokens.FirstOrDefaultAsync(x => x.Token == refreshToken);

            if(tokenDetail is null || tokenDetail.IsActive is false)
            {
                throw new KeyNotFoundException();
            }

            return tokenDetail.UserId;

        }
    }
}
