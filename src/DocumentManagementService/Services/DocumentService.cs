using Azure.Core;
using DocumentManagementService.Data;
using DocumentManagementService.Domain;
using Microsoft.EntityFrameworkCore;
using Shared.Common.Enums;
using Shared.Common.Interfaces;
using Shared.Models;
using System.Reflection.Metadata.Ecma335;
using System.Threading;

namespace DocumentManagementService.Services
{
    internal class DocumentService(ApplicationDbContext dbContext, ILogger<DocumentService> _logger) : IDocumentService
    {
        public async Task<DocumentInfo?> CreateContainerAsync(Guid userId, Guid subscriptionId, CreateContainer container, CancellationToken token = default)
        {
            var Container = new LynkContainer
            {
                Id = Guid.NewGuid(),
                Name = container.Name.Trim(),
                ParentId = container.ParentId,
                SubscriptionId = subscriptionId,
                ModifiedOn = DateTime.UtcNow,
                UserId = userId
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

        public async Task DeleteAsync(Guid userId, Guid subscriptionId, Guid referenceId, CancellationToken token = default)
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

        public async Task EmptyRecycleBinAsync(Guid userId, Guid subscriptionId, CancellationToken token = default)
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

        public async Task<DocumentInfo?> GetDocumentDetailsAsync(Guid userId, Guid subscriptionId, Guid containerId, CancellationToken token = default)
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

        public async Task PurgeRecycleBinItemsAsync(Guid userId, Guid subscriptionId, RecycleBinItem model, CancellationToken token = default)
        {
            if (model.Items == null || !model.Items.Any())
            {
                return;
            }
            var items = model.Items;
            await dbContext.PendingPurge.AddRangeAsync(items.Select(item => new PendingPurge
            {
                ReferenceId = item,
                SubscriptionId = subscriptionId,
                EntityType = EntityType.Document
            }), token);
            await dbContext.SaveChangesAsync(token);

        }

        public async Task<QuerySet<DocumentInfo>> QueryContentAsync(Guid userId, Guid subscriptionId, Guid? ContainerId = null, int skip = 0, int take = 10, string? search = "", string? orderBy = null, bool descending = false, CancellationToken cancellationToken = default)
        {
            var Containers = dbContext.Containers
                .Where(f => f.SubscriptionId == subscriptionId && f.UserId == userId && ((f.ParentId == null && ContainerId == null) || f.ParentId == ContainerId))
                .Select(f => new { f.Id, Name = f.Name!, IsContainer = true, ModifiedOn = f.ModifiedOn, Type = "" });

            var documents = dbContext.Documents
                .Where(d => d.SubscriptionId == subscriptionId && d.UserId == userId && ((d.ContainerId == null && ContainerId == null) || d.ContainerId == ContainerId))
                .Select(d => new { d.Id, Name = d.Name!, IsContainer = false, ModifiedOn = d.ModifiedOn, Type = d.Type! });

            var query = Containers.Union(documents);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(f => f.Name.Contains(search));
            }

            query = orderBy?.ToLower() switch
            {
                "name" => !descending ? query.OrderByDescending(x => x.IsContainer).ThenBy(f => f.Name) : query.OrderByDescending(x => x.IsContainer).ThenByDescending(f => f.Name),
                "modifiedon" => !descending ? query.OrderByDescending(x => x.IsContainer).ThenBy(f => f.ModifiedOn) : query.OrderByDescending(x => x.IsContainer).ThenByDescending(f => f.ModifiedOn),
                _ => query.OrderBy(f => f.Name)
            };

            var count = await query.CountAsync();
            query = query.Skip(skip).Take(take); 
            
            var pinned = dbContext.PinnedObjects.IgnoreQueryFilters();

            var query1 = query.LeftJoin(pinned, a => a.Id, b => b.ReferenceId, (co, a) => new
            {
                co.Id,
                co.Name,
                co.IsContainer,
                co.ModifiedOn,
                IsPinned = a != null
            });

            var items = await query1.Select(x => new DocumentInfo
            {
                Id = x.Id,
                Name = x.Name,
                IsContainer = x.IsContainer,
                ModifiedOn = x.ModifiedOn,
                IsPinned = x.IsPinned
            }).ToListAsync();

            return new QuerySet<DocumentInfo>
            {
                Items = items,
                TotalCount = count
            };
        }

        public async Task<QuerySet<DocumentInfo>> QueryDeletedAsync(Guid userId, Guid subscriptionId, int skip = 0, int take = 10, string search = "", string? orderBy = null, bool descending = false, CancellationToken cancellationToken = default)
        {
            var readyForPurge = dbContext.PendingPurge.IgnoreQueryFilters()
            .Where(f => f.SubscriptionId == subscriptionId)
            .Select(f => new { f.ReferenceId });

            var Containers = dbContext.Containers.IgnoreQueryFilters()
                .Where(f => f.SubscriptionId == subscriptionId && f.UserId == userId && f.ParentId == null && f.IsDeleted && !readyForPurge.Select(r => r.ReferenceId).Contains(f.Id))
                .Select(f => new { f.Id, f.Name, IsContainer = true, f.DeletedOn, f.ModifiedOn });

            var documents = dbContext.Documents
            .IgnoreQueryFilters()
            .Where(d => d.SubscriptionId == subscriptionId && d.UserId == userId && d.IsDeleted && !readyForPurge.Select(r => r.ReferenceId).Contains(d.Id))
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

        public async Task RestoreAsync(Guid userId, Guid subscriptionId, RecycleBinItem model, CancellationToken token = default)
        {
            await dbContext.Containers.IgnoreQueryFilters()
            .Where(x => x.SubscriptionId == subscriptionId && x.UserId == userId && x.IsDeleted && model.Items.Contains(x.Id))
            .ExecuteUpdateAsync(x =>
            {
                x.SetProperty(c => c.IsDeleted, false);
                x.SetProperty(c => c.DeletedOn, (DateTime?)null);
            });

            await dbContext.Documents.IgnoreQueryFilters()
            .Where(x => x.SubscriptionId == subscriptionId && x.UserId == userId && x.IsDeleted && model.Items.Contains(x.Id))
            .ExecuteUpdateAsync(x =>
            {
            });

        }

        public Task<List<DocumentInfo>> GetPinnedDocumentsAsync(Guid userId, Guid subscriptionId, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public async Task PinAsync(Guid userId, Guid subscriptionId, PinRequest pin)
        {
            await dbContext.PinnedObjects.AddAsync(new LynkPin
            {
                UserId = userId,
                Entity = pin.Entity,
                ReferenceId = pin.ReferenceId,
                SubscriptionId = subscriptionId
            });
            await dbContext.SaveChangesAsync();
        }

        public async Task UnpinAsync(Guid userId, Guid subscriptionId, PinRequest pin)
        {
            var existingPin = await dbContext.PinnedObjects
                .FirstOrDefaultAsync(p => p.UserId == userId && p.Entity == pin.Entity && p.ReferenceId == pin.ReferenceId && p.SubscriptionId == subscriptionId);
           
            if (existingPin != null)
            {
                dbContext.PinnedObjects.Remove(existingPin);

                await dbContext.SaveChangesAsync();
            }
        }
    }
}
