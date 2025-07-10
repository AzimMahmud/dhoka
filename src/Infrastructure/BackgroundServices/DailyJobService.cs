using Domain.Posts;
using Infrastructure.Posts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.BackgroundServices;

public class DailyJobService(
    IServiceProvider provider,
    ILogger<DailyJobService> logger)
    : BackgroundService
{

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("DeleteInitPostsHostedService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            // Calculate next 11:00 PM local time
            DateTime now     = DateTime.Now;
            DateTime todayAtFourTwelve = DateTime.Today
                .AddHours(16)    // 4 PM
                .AddMinutes(25); // :12
            DateTime nextRun = now <= todayAtFourTwelve
                ? todayAtFourTwelve
                : todayAtFourTwelve.AddDays(1);

            TimeSpan delay = nextRun - now;
            logger.LogInformation("Next run scheduled at {NextRun}", nextRun);

            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                // Application is shutting down
                break;
            }

            if (stoppingToken.IsCancellationRequested)
            {
                break;
            }

            try
            {
                logger.LogInformation("Starting delete‐init‐posts job at {Time}", DateTime.Now);

                // Create a scoped PostRepository and call the delete method
                using IServiceScope scope = provider.CreateScope();
                IPostRepository repo = scope.ServiceProvider.GetRequiredService<IPostRepository>();
                    
                await repo.DeleteAllInitPostsAndImagesAsync(stoppingToken);

                logger.LogInformation("Finished delete‐init‐posts job at {Time}", DateTime.Now);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while running delete‐init‐posts job.");
            }
        }

        logger.LogInformation("DeleteInitPostsHostedService is stopping.");
    }
}
