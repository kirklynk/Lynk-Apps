using DocumentManagementService.Data;
using DocumentManagementService.Domain;
using Microsoft.EntityFrameworkCore;

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
                    await ProcessPendingPurgeAsync();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred during cleanup.");
                }
                // Wait for a specified interval before the next cleanup
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }

        private async Task ProcessPendingPurgeAsync()
        {
            logger.LogInformation("Starting cleanup of old documents.");
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var pendingDocumentDeletions = await dbContext.PendingPurge.Where(pp => pp.EntityType == PendingPurgeEntityType.Document)
                .AsNoTracking()
                .Select(x => x.ReferenceId)
                .ToListAsync();

            await ExecuteDeleteDocumentsAsync(dbContext, pendingDocumentDeletions);

            var pendingContainerDeletions = await dbContext.PendingPurge.Where(pp => pp.EntityType == PendingPurgeEntityType.Container)
                .AsNoTracking()
                .Select(x => x.ReferenceId)
                .ToListAsync();

            foreach (var containerId in pendingContainerDeletions)
            {
                await DeleteContainerAsync(dbContext, containerId);
            }

            // Remove processed entries from PendingPurge table
            var allPendingDeletions = pendingDocumentDeletions.Concat(pendingContainerDeletions).ToList();

            await dbContext.PendingPurge
                .Where(pp => allPendingDeletions.Contains(pp.ReferenceId))
                .ExecuteDeleteAsync();

            logger.LogInformation("Cleanup of old documents completed.");
        }

        private async Task DeleteContainerAsync(ApplicationDbContext dbContext, Guid containerId)
        {
            var container = await dbContext.Containers
                .IgnoreQueryFilters()
                .Include(c => c.Children)
                .FirstOrDefaultAsync(c => c.Id == containerId);

            if (container == null)
                return;

            // Recursively delete child containers
            foreach (var child in container.Children)
            {
                {
                    await DeleteContainerAsync(dbContext, child.Id);
                }
            }
            // Delete documents in the container
            var documents = await dbContext.Documents
                .IgnoreQueryFilters()
                .Where(d => d.ContainerId == container.Id)
                .Select(x => x.Id)
                .ToListAsync();

            await ExecuteDeleteDocumentsAsync(dbContext, documents);

            // Delete the container itself
            dbContext.Containers.Remove(container);
            await dbContext.SaveChangesAsync();
        }

        private async Task ExecuteDeleteDocumentsAsync(ApplicationDbContext dbContext, List<Guid> pendingDeletions)
        {
            try
            {
                // Delete standalone documents marked for deletion
                var documents = await dbContext.Documents
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .Where(d => pendingDeletions.Contains(d.Id))
                    .Select(x => x.Location)
                    .ToListAsync();

                documents.ForEach(doc =>
                {
                    logger.LogInformation("Deleting document at location: {Location}", doc);
                    if (File.Exists(doc))
                        File.Delete(doc);
                });

                await dbContext.Documents
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .Where(d => pendingDeletions.Contains(d.Id))
                    .ExecuteDeleteAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
