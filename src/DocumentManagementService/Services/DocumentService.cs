using Azure.Core;
using DocumentManagementService.Data;
using DocumentManagementService.Domain;
using DocumentManagementService.Exceptions;
using Microsoft.EntityFrameworkCore;
using Shared.Common.Enums;
using Shared.Common.Interfaces;
using Shared.Models;
using System.Reflection.Metadata.Ecma335;
using System.Threading;

namespace DocumentManagementService.Services
{
    internal class DocumentService(ApplicationDbContext dbContext, ILogger<DocumentService> _logger) : IApiDocumentService
    {
        public async Task<DocumentInfo?> CreateContainerAsync(Guid userId, Guid subscriptionId, CreateContainer container, CancellationToken token = default)
        {

            _logger.LogDebug("Creating container {ContainerName} for subscription {SubscriptionId}", container.Name, subscriptionId);



            var userSubscriptionId = await GetUserSubscriptionIdAsync(userId, subscriptionId, token);

            var found = await dbContext.Containers.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Name == container.Name.Trim() && x.UserSubscriptionId == userSubscriptionId && (x.ParentId == null || (container.ParentId.HasValue && x.ParentId == container.ParentId.Value)));

            if (found != null)
            {
                if (found.IsDeleted)
                {
                    throw new DeletedException("Cannot create a new {{Container}}+ with the same name as a deleted one.");
                }
                throw new ExistingException("A Container with the same name already exists.");
            }

            var Container = new LynkContainer
            {
                Id = Guid.NewGuid(),
                Name = container.Name.Trim(),
                ParentId = container.ParentId,
                UserSubscriptionId = userSubscriptionId,
                ModifiedOn = DateTime.UtcNow
            };

            dbContext.Containers.Add(Container);
            await dbContext.SaveChangesAsync(token);

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

            var userSubscriptionId = await GetUserSubscriptionIdAsync(userId, subscriptionId, token);

            var Container = await dbContext.Containers.Include(x => x.Documents).FirstOrDefaultAsync(x => x.Id == referenceId && x.UserSubscriptionId == userSubscriptionId, token);

            if (Container != null)
            {
                Container.IsDeleted = true;
                Container.DeletedOn = DateTime.UtcNow;
            }
            else
            {
                var document = await dbContext.Documents.FirstOrDefaultAsync(x => x.Id == referenceId && x.UserSubscriptionId == userSubscriptionId, token);
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
            var userSubscriptionId = await GetUserSubscriptionIdAsync(userId, subscriptionId, token);

            var pendingPurge = dbContext.PendingPurge.IgnoreQueryFilters()
             .Where(x => x.SubscriptionId == subscriptionId)
             .Select(x => x.ReferenceId);

            var containers = dbContext.Containers.IgnoreQueryFilters().Where(x => x.UserSubscriptionId == userSubscriptionId && x.IsDeleted && !pendingPurge.Contains(x.Id));

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

            var documents = dbContext.Documents.IgnoreQueryFilters().Where(x => x.UserSubscriptionId == userSubscriptionId && x.IsDeleted && !pendingPurge.Contains(x.Id));

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

        public async Task<DocumentInfo?> GetDetailsAsync(Guid userId, Guid subscriptionId, Guid containerId, CancellationToken token = default)
        {
            var userSubscriptionId = await GetUserSubscriptionIdAsync(userId, subscriptionId, token);
            var container = await dbContext.Containers.Include(x => x.Parent).ThenInclude(x => x.Parent)
                 .Where(x => x.Id == containerId && x.UserSubscriptionId == userSubscriptionId)
                 .FirstOrDefaultAsync();

            return container != null ? new DocumentInfo
            {
                Id = container.Id,
                Name = container.Name,
                Parent = container.Parent != null ? new DocumentInfo
                {
                    Id = container.Parent.Id,
                    Name = container.Parent.Name,
                    Parent = container.Parent.Parent != null ? new DocumentInfo
                    {
                        Id = container.Parent.Parent.Id,
                        Name = container.Parent.Parent.Name
                    } : null
                } : null
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

        public async Task<QuerySet<DocumentInfo>> QueryContentAsync(Guid userId, Guid subscriptionId, Guid? ContainerId = null, int skip = 0, int take = 10, string? search = "", string? orderBy = null, bool descending = false, CancellationToken token = default)
        {
            var userSubscriptionId = await GetUserSubscriptionIdAsync(userId, subscriptionId, token);

            var Containers = dbContext.Containers
                .Where(f => f.UserSubscriptionId == userSubscriptionId && ((f.ParentId == null && ContainerId == null) || f.ParentId == ContainerId))
                .Select(f => new DocumentInfo { Id = f.Id, Name = f.Name!, IsContainer = true, ModifiedOn = f.ModifiedOn, Type = "", Size = 0, Extension = "" });

            var documents = dbContext.Documents
                .Where(d => d.UserSubscriptionId == userSubscriptionId && ((d.ContainerId == null && ContainerId == null) || d.ContainerId == ContainerId))
                .Select(d => new DocumentInfo { Id = d.Id, Name = d.Name!, IsContainer = false, ModifiedOn = d.ModifiedOn, Type = d.Type!, Size = d.Size ?? 0, Extension = d.Extension });

            var query = Containers.Union(documents);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(f => f.Name.Contains(search));
            }

            query = orderBy?.ToLower() switch
            {
                "name" => !descending ? query.OrderByDescending(x => x.IsContainer == true).ThenBy(f => f.Name) : query.OrderByDescending(x => x.IsContainer).ThenByDescending(f => f.Name),
                "modifiedon" => !descending ? query.OrderByDescending(x => x.IsContainer == true).ThenBy(f => f.ModifiedOn) : query.OrderByDescending(x => x.IsContainer == true).ThenByDescending(f => f.ModifiedOn),
                _ => query.OrderByDescending(f => f.IsContainer).ThenBy(f => f.Name)
            };

            var count = await query.CountAsync(cancellationToken: token);
            query = query.Skip(skip).Take(take);

            var pinned = dbContext.PinnedObjects;

            var query1 = query.LeftJoin(pinned, a => a.Id, b => b.ReferenceId, (doc, a) => new DocumentInfo
            {
                Id = doc.Id,
                Name = doc.Name!,
                IsContainer = doc.IsContainer,
                ModifiedOn = doc.ModifiedOn,
                Type = doc.Type!,
                Size = doc.Size,
                IsPinned = a != null,
                Extension = doc.Extension
            });

            var items = await query1.ToListAsync(cancellationToken: token);

            return new QuerySet<DocumentInfo>
            {
                Items = items,
                TotalCount = count
            };
        }

        public async Task<QuerySet<DocumentInfo>> QueryDeletedAsync(Guid userId, Guid subscriptionId, int skip = 0, int take = 10, string search = "", string? orderBy = null, bool descending = false, CancellationToken cancellationToken = default)
        {
            var userSubscriptionId = await GetUserSubscriptionIdAsync(userId, subscriptionId, cancellationToken);

            var readyForPurge = dbContext.PendingPurge.IgnoreQueryFilters()
            .Where(f => f.SubscriptionId == subscriptionId)
            .Select(f => new { f.ReferenceId });

            var Containers = dbContext.Containers.IgnoreQueryFilters()
                .Where(f => f.UserSubscriptionId == userSubscriptionId && f.ParentId == null && f.IsDeleted && !readyForPurge.Select(r => r.ReferenceId).Contains(f.Id))
                .Select(f => new { f.Id, f.Name, IsContainer = true, f.DeletedOn, f.ModifiedOn });

            var documents = dbContext.Documents
            .IgnoreQueryFilters()
            .Where(d => d.UserSubscriptionId == userSubscriptionId && d.IsDeleted && !readyForPurge.Select(r => r.ReferenceId).Contains(d.Id))
                .Select(d => new { d.Id, d.Name, IsContainer = false, d.DeletedOn, d.ModifiedOn });

            var query = Containers.Union(documents);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(f => f.Name.Contains(search));
            }

            query = orderBy?.ToLower() switch
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
            var userSubscriptionId = await GetUserSubscriptionIdAsync(userId, subscriptionId, token);

            await dbContext.Containers.IgnoreQueryFilters()
            .Where(x => x.UserSubscriptionId == userSubscriptionId && x.IsDeleted && model.Items.Contains(x.Id))
            .ExecuteUpdateAsync(x =>
            {
                x.SetProperty(c => c.IsDeleted, false);
                x.SetProperty(c => c.DeletedOn, (DateTime?)null);
            });

            await dbContext.Documents.IgnoreQueryFilters()
            .Where(x => x.UserSubscriptionId == userSubscriptionId && x.IsDeleted && model.Items.Contains(x.Id))
            .ExecuteUpdateAsync(x =>
            {
                x.SetProperty(c => c.IsDeleted, false);
                x.SetProperty(c => c.DeletedOn, (DateTime?)null);
            });

        }

        public async Task<List<DocumentInfo>> GetPinnedItemsAsync(Guid userId, Guid subscriptionId, CancellationToken token = default)
        {
            var userSubscriptionId = await GetUserSubscriptionIdAsync(userId, subscriptionId, token);

            var pins = dbContext.PinnedObjects.Where(x => x.UserSubscriptionId == userSubscriptionId);

            var documents = dbContext.Documents.Where(x => x.UserSubscriptionId == userSubscriptionId).Join(pins, d => d.Id, pinned => pinned.ReferenceId, (d, p) => new { d.Id, d.Name, IsContainer = false, d.DeletedOn, d.ModifiedOn });

            var containers = dbContext.Containers.Where(x => x.UserSubscriptionId == userSubscriptionId).Join(pins, d => d.Id, pinned => pinned.ReferenceId, (d, p) => new { d.Id, d.Name, IsContainer = true, d.DeletedOn, d.ModifiedOn });

            return await documents
                .Union(containers)
                .OrderByDescending(x => x.IsContainer)
                .ThenBy(x => x.Name)
                .Select(x => new DocumentInfo
                {
                    Id = x.Id,
                    Name = x.Name,
                    IsContainer = x.IsContainer,
                    DeletedOn = x.DeletedOn,
                    ModifiedOn = x.ModifiedOn
                }).ToListAsync(token);
        }

        public async Task PinAsync(Guid userId, Guid subscriptionId, PinRequest pin)
        {
            var userSubscriptionId = await GetUserSubscriptionIdAsync(userId, subscriptionId);
            await dbContext.PinnedObjects.AddAsync(new LynkPin
            {
                UserSubscriptionId = userSubscriptionId,
                Entity = pin.Entity,
                ReferenceId = pin.ReferenceId
            });
            await dbContext.SaveChangesAsync();
        }

        public async Task UnpinAsync(Guid userId, Guid subscriptionId, List<PinRequest> pins)
        {
            var userSubscriptionId = await GetUserSubscriptionIdAsync(userId, subscriptionId);
            var removed = pins.Select(x => x.ReferenceId);

            await dbContext.PinnedObjects
               .Where(x => x.UserSubscriptionId == userSubscriptionId && removed.Contains(x.ReferenceId))
               .ExecuteDeleteAsync();
        }

        public async Task<LynkDocument?> GetAsync(Guid userId, Guid subscriptionId, Guid id, CancellationToken token = default)
        {
            var userSubscriptionId = await GetUserSubscriptionIdAsync(userId, subscriptionId, token);
            return await dbContext.Documents
                .Where(x => x.Id == id && x.UserSubscriptionId == userSubscriptionId)
                .FirstOrDefaultAsync(token);
        }

        private async Task<Guid> GetUserSubscriptionIdAsync(Guid userId, Guid subscriptionId, CancellationToken token = default)
        {
            return await dbContext.Database.SqlQueryRaw<Guid>("SELECT Id AS Value FROM UserSubscriptions WHERE UserId = {0} AND SubscriptionId = {1}", userId, subscriptionId).FirstAsync(token);
        }

        public async Task<DocumentInfo> AddDocumentAsync(Guid userId, Guid subscriptionId, string fileNameNoExtension, string extension, string container, string path, string contentType, long length, CancellationToken token)
        {
            var friendlyName = fileNameNoExtension;
            int index = 0;
            var userSubscriptionId = await GetUserSubscriptionIdAsync(userId, subscriptionId, token);

            var query = dbContext.Documents.Where(x => x.UserSubscriptionId == userSubscriptionId).AsQueryable();

            if (Guid.TryParse(container, out var containerId))
            {
                query = query.Where(x => x.ContainerId == containerId);
            }

            while (await query.Where(x => x.Name == friendlyName).AnyAsync())
            {
                friendlyName = $"{Path.GetFileNameWithoutExtension(fileNameNoExtension)} ({++index})";
            }

            var doc = new LynkDocument(friendlyName)
            {
                Id = Guid.NewGuid(),
                ContainerId = containerId == Guid.Empty ? null : containerId,
                Location = path,
                UserSubscriptionId = userSubscriptionId,
                ModifiedOn = DateTime.UtcNow,
                Type = contentType,
                Extension = extension,
                Size = length
            };

            await dbContext.Documents.AddAsync(doc);

            await dbContext.SaveChangesAsync();
            return new DocumentInfo
            {
                Id = doc.Id,
                Name = doc.Name,
                IsContainer = false,
                ModifiedOn = doc.ModifiedOn,
                Type = doc.Type,
                Size = doc.Size ?? 0,
                Extension = doc.Extension
            };
        }
    }
}
