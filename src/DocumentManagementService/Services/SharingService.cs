using DocumentManagementService.Data;
using Microsoft.EntityFrameworkCore;
using Shared.Common.Enums;
using Shared.Common.Interfaces;
using Shared.Models;

namespace DocumentManagementService.Services
{
    internal class SharingService(ApplicationDbContext dbContext, ILogger<SharingService> logger, IHttpContextAccessor httpContextAccessor) : ISharingService
    {
        public async Task<QuerySet<ShareRequest>> QueryAsync(Guid userId, Guid subscriptionId, int skip, int take, string? orderBy, bool descending, CancellationToken cancellationToken)
        {
            var query = dbContext.Shares
                .Where(s => s.EntityType == EntityType.Document)
                .Join(
                    dbContext.Documents,
                    share => share.ReferenceId,
                    document => document.Id,
                    (share, d) => new ShareRequest
                    {
                        Id = share.Id,
                        Name = d.Name,
                        //share.Description,
                        Owner = share.Owner,
                        EntityType = share.EntityType,
                        ReferenceId = share.ReferenceId
                    });

            var sharedContainers = dbContext.Shares
                .Where(s => s.EntityType == EntityType.Container)
                .Join(
                    dbContext.Containers,
                    share => share.ReferenceId,
                    container => container.Id,
                    (share, c) => new ShareRequest
                    {
                        Id = share.Id,
                        Name = c.Name,
                        //share.Description,
                        Owner = share.Owner,
                        EntityType = share.EntityType,
                        ReferenceId = share.ReferenceId
                    }
                    );

            var union = query.Union(sharedContainers);

            var count = await union.CountAsync(cancellationToken);
            return new QuerySet<ShareRequest>
            {
                Items = await union.ToListAsync(cancellationToken),
                TotalCount = count
            };

        }
    }
}
