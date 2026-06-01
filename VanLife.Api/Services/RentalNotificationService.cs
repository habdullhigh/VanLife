using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace VanLife.Api.Services;

public class RentalNotificationService : BackgroundService
{
    private readonly IServiceProvider services;
    private readonly ILogger<RentalNotificationService> logger;
    private readonly TimeSpan scheduledTime = TimeSpan.FromHours(8); // 8:00 AM

    public RentalNotificationService(IServiceProvider services, ILogger<RentalNotificationService> logger)
    {
        this.services = services;
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Calculate initial delay until next scheduledTime
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.UtcNow;
                var next = now.Date + scheduledTime;
                if (next <= now) next = next.AddDays(1);

                var delay = next - now;
                logger.LogInformation("RentalNotificationService sleeping until {Next} (UTC)", next.ToString("o", CultureInfo.InvariantCulture));
                await Task.Delay(delay, stoppingToken);

                await DoWorkAsync(stoppingToken);
            }
            catch (TaskCanceledException)
            {
                // shutting down
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in RentalNotificationService loop");
                // Wait a minute before retrying to avoid tight exception loop
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }

    private async Task DoWorkAsync(CancellationToken cancellationToken)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<VanLife.Api.Data.AppDbContext>();
        var emailer = scope.ServiceProvider.GetRequiredService<IEmailService>();

        var utcNow = DateTime.UtcNow.Date;

        // Overdue rentals: EndDate < today
        var overdue = await db.Rentals
            .Include(r => r.Buyer)
            .Where(r => r.EndDate != default && r.EndDate < utcNow)
            .ToListAsync(cancellationToken);

        foreach (var rent in overdue)
        {
            // Recalculate fine if not frozen
            if (!rent.FineFrozen)
            {
                var overdueDays = 0;
                var compareDate = DateTime.UtcNow.Date;
                var graceEnd = rent.EndDate.AddDays(rent.FineGraceDays);
                if (compareDate > graceEnd.Date)
                {
                    overdueDays = (int)(compareDate - rent.EndDate.Date).TotalDays;
                }
                rent.FineAmount = overdueDays * rent.FineRate;
                await db.SaveChangesAsync(cancellationToken);
            }

            // Try to resolve an email address from the Users table if Buyer exists
            var to = string.Empty;
            if (rent.Buyer is not null)
            {
                var user = await db.Users.FirstOrDefaultAsync(u => u.Id == rent.Buyer.BuyerId, cancellationToken);
                if (user is not null && !string.IsNullOrWhiteSpace(user.Email)) to = user.Email;
            }
            if (string.IsNullOrWhiteSpace(to)) to = rent.Contact ?? string.Empty;

            var subject = "Overdue rental - Fine may apply";
            var body = $"Your rental (PurchaseId: {rent.PurchaseId}) for van {rent.VanId} was due on {rent.EndDate:yyyy-MM-dd}. Current fine: {rent.FineAmount} {rent.FineCurrency}. Please return the van and contact the seller.";
            try
            {
                if (!string.IsNullOrWhiteSpace(to))
                {
                    await emailer.SendEmailAsync(to, subject, body);
                }
                else
                {
                    logger.LogInformation("No contact email for overdue rental {Id}", rent.PurchaseId);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send overdue email for rental {Id}", rent.PurchaseId);
            }
        }

        // Reminder rentals: EndDate within next 14 days (including today)
        var reminderThreshold = utcNow.AddDays(14);
        var reminders = await db.Rentals
            .Include(r => r.Buyer)
            .Where(r => r.EndDate != default && r.EndDate >= utcNow && r.EndDate <= reminderThreshold)
            .ToListAsync(cancellationToken);

        foreach (var rent in reminders)
        {
            var to = string.Empty;
            if (rent.Buyer is not null)
            {
                var user = await db.Users.FirstOrDefaultAsync(u => u.Id == rent.Buyer.BuyerId, cancellationToken);
                if (user is not null && !string.IsNullOrWhiteSpace(user.Email)) to = user.Email;
            }
            if (string.IsNullOrWhiteSpace(to)) to = rent.Contact ?? string.Empty;
            var subject = "Upcoming rental end reminder";
            var body = $"Your rental (PurchaseId: {rent.PurchaseId}) for van {rent.VanId} will end on {rent.EndDate:yyyy-MM-dd}. Please be ready to return the van. If you keep the van beyond the end date, fines may apply.";
            try
            {
                if (!string.IsNullOrWhiteSpace(rent.Buyer?.Username))
                {
                    await emailer.SendEmailAsync(rent.Buyer!.Username, subject, body);
                }
                else if (!string.IsNullOrWhiteSpace(rent.Contact))
                {
                    await emailer.SendEmailAsync(rent.Contact, subject, body);
                }
                else
                {
                    logger.LogInformation("No contact email for reminder rental {Id}", rent.PurchaseId);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send reminder email for rental {Id}", rent.PurchaseId);
            }
        }

        logger.LogInformation("RentalNotificationService completed sending notifications at {Now}", DateTime.UtcNow);
    }
}
