using Shared.Models;
using Shared.Requests;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Common.Interfaces
{
    public interface IDocumentService
    {
        Task<DocumentInfo?> CreateContainerAsync(Guid subscriptionId, CreateContainer Container);

        Task<QuerySet<DocumentInfo>> QueryAsync(Guid subscriptionId, Guid? ContainerId = null, int skip = 0, int take = 10, string? search = "",
            string? orderBy = null, bool descending = false, CancellationToken cancellationToken = default);

        Task<DocumentInfo?> GetDetailsAsync(Guid subscriptionId, Guid ContainerId);

        Task DeleteAsync(Guid subscriptionId, Guid referenceId);

        Task EmptyRecycleBinAsync(Guid subscriptionId);

        Task<QuerySet<DocumentInfo>> QueryDeletedAsync(Guid subscriptionId, int skip = 0, int take = 10, string search = "", string? orderBy = null, bool descending = false, CancellationToken cancellationToken = default);
        Task RestoreAsync(Guid subscriptionId, Restore model);
    }
}
