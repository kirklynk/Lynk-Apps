using Shared.Models;
using Shared.Requests;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Common.Interfaces
{
    public interface IFileService
    {
        Task<FileDetails> CreateFolderAsync(Guid subscriptionId, CreateFolder folder);

        Task<QuerySet<LynkFileInfo>> QueryAsync(Guid subscriptionId, Guid? folderId = null, int startIdx = 0, int pageSize = 10, string search = "");

        Task<FileDetails> GetDetailsAsync(Guid subscriptionId, Guid folderId);
    }
}
