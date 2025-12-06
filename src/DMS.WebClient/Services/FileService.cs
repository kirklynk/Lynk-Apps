using MudBlazor;
using Shared.Common.Interfaces;
using Shared.Models;
using Shared.Requests;
using System.Net.Http;
using System.Net.Http.Json;

namespace DMS.WebClient.Services
{
    public class FileService(IHttpClientFactory factory) : IFileService
    {
        HttpClient _httpClient => factory.CreateClient("backend");
        public async Task<FileDetails> CreateFolderAsync(Guid subscriptionId, CreateFolder folder)
        {
            using var response = await _httpClient.PostAsJsonAsync($"/dms/api/{subscriptionId}/files/folders", folder);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<FileDetails>();
        }

        public async Task<FileDetails> GetDetailsAsync(Guid subscriptionId, Guid folderId)
        {
            using var response = await _httpClient.GetAsync($"/dms/api/{subscriptionId}/files/folders/{folderId}/details");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<FileDetails>();
        }

        public async Task<QuerySet<LynkFileInfo>> QueryAsync(Guid subscriptionId, Guid? folderId = null, int startIndex = 0, int pageSize = 10, string search = "")
        {

            string url = folderId.HasValue ? $"/dms/api/{subscriptionId}/files/folders/{folderId}" : $"/dms/api/{subscriptionId}/files";

            var filters = new Dictionary<string, string>();
            filters["page"] = startIndex.ToString();
            filters["pageSize"] = pageSize.ToString();
            string queryString = "";
            if (!string.IsNullOrEmpty(search))
            {
                filters["search"] = search;
            }
            queryString = string.Join("&", filters.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));

            if (!string.IsNullOrEmpty(queryString))
            {
                url += $"?{queryString}";
            }
            var response = await _httpClient.GetFromJsonAsync<QuerySet<LynkFileInfo>>(url) ?? new QuerySet<LynkFileInfo>();
            return response;
        }
    }
}
