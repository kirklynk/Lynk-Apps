
namespace DocumentManagementService.Services
{
    internal class DiskFileStorageService : IFileStorageService
    {
        public Task<Stream> GetStreamAsync(string location, CancellationToken token)
        {
            FileStream fileStream = new FileStream(location, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
            return Task.FromResult<Stream>(fileStream);
        }

        public async Task<string> SaveFileAsync(IFormFile file, CancellationToken token = default)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is not valid.", nameof(file));
            }

            var uploadPath = "c:\\temp\\uploads";
            var directoryInfo = new DirectoryInfo(uploadPath);

            if (directoryInfo.Exists == false)
            {
                directoryInfo.Create();
            }

            var extension = ".dat";
            var name = $"{Guid.NewGuid()}{extension}";
            var path = Path.Combine(directoryInfo.FullName, name);

            await using var fs = new FileStream(
              path,
              FileMode.Create,
              FileAccess.Write,
              FileShare.None,
              bufferSize: 1024 * 1024,
              useAsync: true);

            await file.CopyToAsync(fs, token);
            return path;
        }
    }
}
