using Shared.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Common.Interfaces
{
    public interface IDocumentService
    {
        Task<DocumentInfo?> CreateContainerAsync(Guid userId, Guid subscriptionId, CreateContainer Container, CancellationToken token = default);

        Task<QuerySet<DocumentInfo>> QueryAsync(Guid userId, Guid subscriptionId, Guid? ContainerId = null, int skip = 0, int take = 10, string? search = "",
            string? orderBy = null, bool descending = false, CancellationToken cancellationToken = default);

        Task<List<ContainerInfo>> GetRelatedContainersAsync(Guid userId, Guid subscriptionId, Guid ContainerId, CancellationToken token = default);

        Task DeleteAsync(Guid userId, Guid subscriptionId, Guid referenceId, CancellationToken token = default);

        Task EmptyRecycleBinAsync(Guid userId, Guid subscriptionId, CancellationToken token = default);

        Task<QuerySet<DocumentInfo>> QueryDeletedAsync(Guid userId, Guid subscriptionId, int skip = 0, int take = 10, string search = "", string? orderBy = null, bool descending = false, CancellationToken cancellationToken = default);
       
        Task RestoreAsync(Guid userId, Guid subscriptionId, RecycleBinItem model, CancellationToken token = default);

        Task PurgeRecycleBinItemsAsync(Guid userId, Guid subscriptionId, RecycleBinItem model, CancellationToken token = default);

        Task PinAsync(Guid userId, Guid subscriptionId, PinRequest pin);

        Task UnpinAsync(Guid userId, Guid subscriptionId, List<PinRequest> pin);

        Task<List<DocumentInfo>> GetPinnedItemsAsync(Guid userId, Guid subscriptionId, CancellationToken token = default);

    }
}
