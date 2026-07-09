using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class OverdueBookingCleanupService
(ILogger<OverdueBookingCleanupService> logger, IServiceScopeFactory serviceScope, TimeProvider time, IOptions<AppSettings> settings)
: BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {

        using (var timer = new PeriodicTimer(TimeSpan.FromMinutes(settings.Value.OverdueBookingCleanupFrequencyMinutes)))
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                logger.LogInformation("Checking overdue work orders at {currentTime}", time.GetUtcNow());
                try
                {
                    using (var service = serviceScope.CreateScope())
                    {
                        var db = service.ServiceProvider.GetRequiredService<IAppDbContext>();

                        var cutoff = time.GetUtcNow().AddMinutes(-settings.Value.BookingCancellationThresholdMinutes);

                        var WorkOrders = await db.WorkOrders.Where(n => n.State == WorkOrderState.Scheduled && n.StartAtUtc <= cutoff)
                                                            .ToListAsync(stoppingToken);

                        if (WorkOrders.Any())
                        {
                            foreach (var order in WorkOrders)
                            {
                                var result = order.MarkAsCancelled();
                                if (result.IsError)
                                {
                                    logger.LogWarning("Failed to cancel WorkOrder {Id}: {@Error}", order.Id, result.Errors);
                                }
                            }

                            await db.SaveChangesAsync(stoppingToken);
                            logger.LogInformation("Cancelled {Count} overdue work orders: {@Ids}", WorkOrders.Count, WorkOrders.Select(w => w.Id));
                        }
                        else
                        {
                            logger.LogInformation("No overdue work orders found");
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error During Cleaning Up Overdue Work Orders");
                }
            }
        }
    }
}