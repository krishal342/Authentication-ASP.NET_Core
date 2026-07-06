using Authentication.Data;
using Authentication.Models;
using Microsoft.EntityFrameworkCore;

namespace Authentication.Services
{
    public class OtpService(ApplicationDbContext context)
    {
        private readonly ApplicationDbContext _context = context;

        // generate otp
        public async Task<int> GenerateOtpAsync()
        {
            var random = new Random();
            return random.Next(100000, 999999); // Generate a 6-digit OTP
        }

        // store otp in db
        public async Task StoreOtpAsync(int userId, int otp, string otpToken)
        {
            var otpEntity = new Otp
            {
                UserId = userId,
                OTP = otp,
                Token = otpToken,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5) // Set OTP expiration time
            };

            _context.Otps.Add(otpEntity);
            await _context.SaveChangesAsync();
        }



        // verify otp
        public async Task<int> VerifyOtpAsync(string token, int otp)
        {
            var otpEntity = await _context.Otps.FirstOrDefaultAsync(o => o.Token == token && o.ExpiresAt > DateTime.UtcNow);

            if (otpEntity is null)
            {
                throw new UnauthorizedAccessException("Expired or invalid token.");
            }

            if (otpEntity.OTP != otp || otpEntity.IsUsed)
            {
                throw new UnauthorizedAccessException("Invalid OTP.");
            }

            otpEntity.IsUsed = true;
            await _context.SaveChangesAsync();

            return otpEntity.UserId;

        }


        // verify otp token
        public async Task<int> VerifyOtpTokenAsync(string otpToken)
        {
            var otpEntity = await _context.Otps
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Token == otpToken && o.ExpiresAt > DateTime.UtcNow);

            // Check if the OTP entity is null or if OTP has not been used
            if (otpEntity is null || !otpEntity.IsUsed)
            {
                throw new UnauthorizedAccessException("Expired or invalid OTP token. otp is not used");
            }



            return otpEntity.UserId;
        }

        // delete otp
        public async Task DeleteOtpAsync(string otpToken)
        {
            var otpEntity = await _context.Otps.FirstOrDefaultAsync(o => o.Token == otpToken);
            if (otpEntity != null)
            {
                _context.Otps.Remove(otpEntity);
                await _context.SaveChangesAsync();
            }
        }
    }

}

