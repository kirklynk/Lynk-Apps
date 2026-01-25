

namespace DocumentManagementService.Services
{
    internal class AzureBlobFileStorageService : IFileStorageService
    {
        public Task<Stream> GetStreamAsync(string location, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<string> SaveFileAsync(IFormFile file, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }
    }
}
