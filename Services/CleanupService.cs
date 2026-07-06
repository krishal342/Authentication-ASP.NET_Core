using Authentication.Data;
using Microsoft.EntityFrameworkCore;

namespace Authentication.Services
{
    // 1. Refresh token cleanup service
    public class RefreshTokenCleanupService(IServiceScopeFactory scopeFactory, ILogger<RefreshTokenCleanupService> logger) : BackgroundService
    {

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {

                    using var scope = scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    var expiredTokens = await db.RefreshTokens
                        .Where(t => t.ExpiresAt < DateTime.UtcNow).ToListAsync(stoppingToken);

                    if (expiredTokens.Count > 0)
                    {
                        db.RefreshTokens.RemoveRange(expiredTokens);
                        await db.SaveChangesAsync(stoppingToken);

                    }
                    logger.LogInformation("Expired refresh tokens cleaned up successfully.");

                }
                catch
                {
                    logger.LogError("Error occurred while cleaning up expired refresh tokens.");
                }
                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            }
        }
    }

    // 2. OTP cleanup service
    public class OtpCleanupService(IServiceScopeFactory scopeFactory, ILogger<OtpCleanupService> logger) : BackgroundService
    {

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {

                    using var scope = scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    var expiredOtps = await db.Otps
                        .Where(o => o.ExpiresAt < DateTime.UtcNow)
                        .ToListAsync(stoppingToken);

                    if (expiredOtps.Count > 0)
                    {
                        db.Otps.RemoveRange(expiredOtps);
                        await db.SaveChangesAsync(stoppingToken);
                    }
                    logger.LogInformation("Expired OTPs cleaned up successfully.");

                }
                catch
                {
                    logger.LogError("Error occurred while cleaning up expired OTPs.");
                }
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }

}
