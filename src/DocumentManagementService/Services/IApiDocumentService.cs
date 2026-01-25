using DocumentManagementService.Domain;
using Shared.Common.Interfaces;
using Shared.Models;

namespace DocumentManagementService.Services
{
    internal interface IApiDocumentService : IDocumentService
    {
        Task<DocumentInfo> AddDocumentAsync(Guid userId, Guid subscriptionId, string fileNameNoExtension, string extension, string container, string path, string contentType, long length, CancellationToken token);
        Task<LynkDocument?> GetAsync(Guid userId, Guid subscriptionId, Guid id, CancellationToken token = default);
    }
}
