
namespace DocumentManagementService.Services
{
    internal interface IFileStorageService
    {
        Task<Stream> GetStreamAsync(string location, CancellationToken token);
        public Task<string> SaveFileAsync(IFormFile file, CancellationToken token = default);
    }
}
