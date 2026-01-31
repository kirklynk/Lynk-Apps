using DocumentManagementService.Data;
using DocumentManagementService.Domain;
using Microsoft.EntityFrameworkCore;
using Shared.Common.Enums;

namespace DocumentManagementService.Services
{
    public class CleanUpService(IServiceProvider serviceProvider, ILogger<CleanUpService> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Perform cleanup tasks here
                   // await ProcessPendingPurgeAsync();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred during cleanup.");
                }
                // Wait for a specified interval before the next cleanup
                await Task.Delay(TimeSpan.FromMinutes(3), stoppingToken);
            }
        }
    }
}
