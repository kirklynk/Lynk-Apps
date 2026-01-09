using Azure.Core;
using DocumentManagementService.Data;
using DocumentManagementService.Domain;
using Microsoft.EntityFrameworkCore;
using Shared.Common.Enums;
using Shared.Common.Interfaces;
using Shared.Models;
using System.Threading;

namespace DocumentManagementService.Services
{
    internal class DocumentService(ApplicationDbContext dbContext, ILogger<DocumentService> _logger) : IDocumentService
    {
        public async Task<DocumentInfo?> CreateContainerAsync(Guid subscriptionId, CreateContainer container, CancellationToken token = default)
        {
            var Container = new LynkContainer
            {
                Id = Guid.NewGuid(),
                Name = container.Name.Trim(),
                ParentId = container.ParentId,
                SubscriptionId = subscriptionId,
                ModifiedOn = DateTime.UtcNow
            };

            dbContext.Containers.Add(Container);
            await dbContext.SaveChangesAsync();

            return new DocumentInfo
            {
                Id = Container.Id,
                Name = Container.Name,
                Parent = container.ParentId.HasValue ? new DocumentInfo { Id = container.ParentId.Value } : null
            };
        }

        public async Task DeleteAsync(Guid subscriptionId, Guid referenceId, CancellationToken token = default)
        {
            _logger.LogDebug("Deleting document or container with ID {ReferenceId} for subscription {SubscriptionId}", referenceId, subscriptionId);

            var Container = await dbContext.Containers.Include(x => x.Documents).FirstOrDefaultAsync(x => x.Id == referenceId && x.SubscriptionId == subscriptionId, token);
            
            if (Container != null)
            {
                Container.IsDeleted = true;
                Container.DeletedOn = DateTime.UtcNow;
            }
            else
            {
                var document = await dbContext.Documents.FirstOrDefaultAsync(x => x.Id == referenceId && x.SubscriptionId == subscriptionId, token);
                if (document != null)
                {
                    document.IsDeleted = true;
                    document.DeletedOn = DateTime.UtcNow;
                }
            }
            await dbContext.SaveChangesAsync(token);
        }

        public async Task EmptyRecycleBinAsync(Guid subscriptionId, CancellationToken token = default)
        {
            var pendingPurge = dbContext.PendingPurge.IgnoreQueryFilters()
             .Where(x => x.SubscriptionId == subscriptionId)
             .Select(x => x.ReferenceId);

            var containers = dbContext.Containers.IgnoreQueryFilters().Where(x => x.SubscriptionId == subscriptionId && x.IsDeleted && !pendingPurge.Contains(x.Id));

            if (await containers.AnyAsync())
            {
                await dbContext.PendingPurge.AddRangeAsync(containers.Select(c => new PendingPurge
                {
                    ReferenceId = c.Id,
                    SubscriptionId = subscriptionId,
                    EntityType = EntityType.Container
                }));
                await dbContext.SaveChangesAsync();
            }

            var documents = dbContext.Documents.IgnoreQueryFilters().Where(x => x.SubscriptionId == subscriptionId && x.IsDeleted && !pendingPurge.Contains(x.Id));

            if (await documents.AnyAsync())
            {
                await dbContext.PendingPurge.AddRangeAsync(documents.Select(d => new PendingPurge
                {
                    ReferenceId = d.Id,
                    SubscriptionId = subscriptionId,
                    EntityType = EntityType.Document
                }));
                await dbContext.SaveChangesAsync();
            }
        }

        public async Task<DocumentInfo?> GetDocumentDetailsAsync(Guid subscriptionId, Guid containerId, CancellationToken token = default)
        {
            var container = await dbContext.Containers.Include(x => x.Parent)
                 .Where(x => x.Id == containerId && x.SubscriptionId == subscriptionId)
                 .FirstOrDefaultAsync();

            return container != null ? new DocumentInfo
            {
                Id = container.Id,
                Name = container.Name,
                Parent = container.Parent != null ? new DocumentInfo { Id = container.Parent.Id, Name = container.Parent.Name } : null
            } : null;
        }

        public Task PurgeRecycleBinItemsAsync(Guid subscriptionId, RecycleBinItem model, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public async Task<QuerySet<DocumentInfo>> QueryContentAsync(Guid subscriptionId, Guid? ContainerId = null, int skip = 0, int take = 10, string? search = "", string? orderBy = null, bool descending = false, CancellationToken cancellationToken = default)
        {
            var Containers = dbContext.Containers
            .Where(f => f.SubscriptionId == subscriptionId && ((f.ParentId == null && ContainerId == null) || f.ParentId == ContainerId))
            .Select(f => new { f.Id, f.Name, IsContainer = true, f.ModifiedOn, Type = "" });

            var documents = dbContext.Documents.Where(d => d.SubscriptionId == subscriptionId && ((d.ContainerId == null && ContainerId == null) || d.ContainerId == ContainerId))
                .Select(d => new { d.Id, d.Name, IsContainer = false, d.ModifiedOn, d.Type });

            var query = Containers.Union(documents);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(f => f.Name.Contains(search));
            }

            query = orderBy.ToLower() switch
            {
                "name" => !descending ? query.OrderByDescending(x => x.IsContainer).ThenBy(f => f.Name) : query.OrderByDescending(x => x.IsContainer).ThenByDescending(f => f.Name),
                "modifiedon" => !descending ? query.OrderByDescending(x => x.IsContainer).ThenBy(f => f.ModifiedOn) : query.OrderByDescending(x => x.IsContainer).ThenByDescending(f => f.ModifiedOn),
                _ => query.OrderBy(f => f.Name)
            };

            var count = await query.CountAsync();
            query = query.Skip(skip).Take(take);
            var items = await query.Select(x => new DocumentInfo
            {
                Id = x.Id,
                Name = x.Name,
                IsContainer = x.IsContainer,
                ModifiedOn = x.ModifiedOn
            }).ToListAsync();

            return new QuerySet<DocumentInfo>
            {
                Items = items,
                TotalCount = count
            };
        }

        public async Task<QuerySet<DocumentInfo>> QueryDeletedAsync(Guid subscriptionId, int skip = 0, int take = 10, string search = "", string? orderBy = null, bool descending = false, CancellationToken cancellationToken = default)
        {
            var readyForPurge = dbContext.PendingPurge.IgnoreQueryFilters()
            .Where(f => f.SubscriptionId == subscriptionId)
            .Select(f => new { f.ReferenceId });
            var Containers = dbContext.Containers.IgnoreQueryFilters()
                .Where(f => f.SubscriptionId == subscriptionId && f.ParentId == null && f.IsDeleted && !readyForPurge.Select(r => r.ReferenceId).Contains(f.Id))
                .Select(f => new { f.Id, f.Name, IsContainer = true, f.DeletedOn, f.ModifiedOn });

            var documents = dbContext.Documents
            .IgnoreQueryFilters()
            .Where(d => d.SubscriptionId == subscriptionId && d.IsDeleted && !readyForPurge.Select(r => r.ReferenceId).Contains(d.Id))
                .Select(d => new { d.Id, d.Name, IsContainer = false, d.DeletedOn, d.ModifiedOn });

            var query = Containers.Union(documents);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(f => f.Name.Contains(search));
            }

            query = orderBy.ToLower() switch
            {
                "name" => !descending ? query.OrderByDescending(x => x.IsContainer).ThenBy(f => f.Name) : query.OrderByDescending(x => x.IsContainer).ThenByDescending(f => f.Name),
                "modifiedon" => !descending ? query.OrderByDescending(x => x.IsContainer).ThenBy(f => f.DeletedOn) : query.OrderByDescending(x => x.IsContainer).ThenByDescending(f => f.DeletedOn),
                _ => query.OrderBy(f => f.Name)
            };

            var count = await query.CountAsync(cancellationToken: cancellationToken);

            query = query.Skip(skip).Take(take);
            return new QuerySet<DocumentInfo>
            {
                Items = await query.Select(x => new DocumentInfo()
                {
                    Id = x.Id,
                    Name = x.Name,
                    IsContainer = x.IsContainer,
                    DeletedOn = x.DeletedOn,
                    ModifiedOn = x.ModifiedOn
                }).ToListAsync(cancellationToken: cancellationToken),
                TotalCount = count
            };
        }

        public Task RestoreAsync(Guid subscriptionId, RecycleBinItem model, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }
    }
}
