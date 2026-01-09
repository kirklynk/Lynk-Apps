using Shared.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Common.Interfaces
{
    public interface IDocumentService
    {
        Task<DocumentInfo?> CreateContainerAsync(Guid subscriptionId, CreateContainer Container, CancellationToken token = default);

        Task<QuerySet<DocumentInfo>> QueryContentAsync(Guid subscriptionId, Guid? ContainerId = null, int skip = 0, int take = 10, string? search = "",
            string? orderBy = null, bool descending = false, CancellationToken cancellationToken = default);

        Task<DocumentInfo?> GetDocumentDetailsAsync(Guid subscriptionId, Guid ContainerId, CancellationToken token = default);

        Task DeleteAsync(Guid subscriptionId, Guid referenceId, CancellationToken token = default);

        Task EmptyRecycleBinAsync(Guid subscriptionId, CancellationToken token = default);

        Task<QuerySet<DocumentInfo>> QueryDeletedAsync(Guid subscriptionId, int skip = 0, int take = 10, string search = "", string? orderBy = null, bool descending = false, CancellationToken cancellationToken = default);
        Task RestoreAsync(Guid subscriptionId, RecycleBinItem model, CancellationToken token = default);

        Task PurgeRecycleBinItemsAsync(Guid subscriptionId, RecycleBinItem model, CancellationToken token = default);
    }
}
