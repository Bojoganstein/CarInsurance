using CarInsurance.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace CarInsurance.Api.ScheduledTasks.PolicyExpiry
{
    public class PolicyExpiryLogger : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<PolicyExpiryLogger> _logger;
        private readonly TimeProvider _time;

        public PolicyExpiryLogger(IServiceScopeFactory scopeFactory, ILogger<PolicyExpiryLogger> logger, TimeProvider time)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _time = time;
        }

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            //var timer = new PeriodicTimer(TimeSpan.FromSeconds(10)); // Added 10 seconds for testing purposes, change to TimeSpan.FromHours(1) in production
            var timer = new PeriodicTimer(TimeSpan.FromHours(1));

            while (await timer.WaitForNextTickAsync(ct))
            {
                await RunOnce(ct);
            }
        }

        private async Task RunOnce(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var now = _time.GetUtcNow().UtcDateTime;
            //var windowStart = now.AddHours(-24); // Added -24 hours for testing purposes, change to now.AddHours(-1) in production
            var windowStart = now.AddHours(-1);

            var expiredPolicies = await db.Policies
                .Where(p => p.EndDate <= DateOnly.FromDateTime(now) &&
                            p.EndDate >= DateOnly.FromDateTime(windowStart))
                .ToListAsync(ct);

            foreach (var ep in expiredPolicies)
            {
                if (await db.ProcessedExpirations.AnyAsync(e => e.PolicyId == ep.Id, ct))
                    continue;

                _logger.LogInformation("Policy {PolicyId} for Car {CarId} expired on {EndDate}", ep.Id, ep.CarId, ep.EndDate);

                db.ProcessedExpirations.Add(new ProcessedExpiration
                {
                    PolicyId = ep.Id,
                    LoggedAtUtc = now
                });
            }

            await db.SaveChangesAsync(ct);
        }
    }
}

