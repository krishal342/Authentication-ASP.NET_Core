using Authentication.Data;

namespace Authentication.Services
{
    public class RefreshTokenCleanupService(IServiceScopeFactory scopeFactory) : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var expiredTokens = db.RefreshTokens.Where(t => t.IsExpired);

                db.RefreshTokens.RemoveRange(expiredTokens);
                await db.SaveChangesAsync();

                await Task.Delay(TimeSpan.FromDays(1),stoppingToken);
            }
        }
    }
}
