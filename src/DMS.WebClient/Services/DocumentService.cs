using DMS.WebClient.Models;
using Shared.Common.Interfaces;
using Shared.Models;
using System.Net.Http.Json;

namespace DMS.WebClient.Services
{
    public class DocumentService(IHttpClientFactory factory, ILogger<DocumentService> logger) : IDocumentService
    {
        readonly HttpClient _httpClient = factory.CreateClient(Constants.DEFAULT_URL_KEY);

        public async Task<DocumentInfo?> CreateContainerAsync(Guid subscriptionId, CreateContainer Container, CancellationToken token = default)
        {
            logger.LogDebug("Creating container {ContainerName}", Container.Name);

            using var response = await _httpClient.PostAsJsonAsync($"/dms/api/{subscriptionId}/documents/containers", Container, token);
            try
            {
                response.EnsureSuccessStatusCode();
                logger.LogDebug("Container {ContainerName} created successfully", Container.Name);
                return await response.Content.ReadFromJsonAsync<DocumentInfo>(token);
            }
            catch (HttpRequestException hre)
            {
                //var content = await response.Content.ReadFromJsonAsync<ProblemDetail>();
                //logger.LogError("Error creating container {ContainerName}: {ErrorDetail}", Container.Name, content?.Detail ?? hre.Message);
                //throw new ApplicationException(content?.Detail ?? hre.Message, hre);
                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task DeleteAsync(Guid subscriptionId, Guid id, CancellationToken token = default)
        {
            using var response = await _httpClient.DeleteAsync($"/dms/api/{subscriptionId}/documents/{id}", token);
            response.EnsureSuccessStatusCode();
        }

        public async Task EmptyRecycleBinAsync(Guid subscriptionId, CancellationToken token = default)
        {
            using var response = await _httpClient.PostAsync($"/dms/api/{subscriptionId}/documents/recyclebin/empty", null, token);
            response.EnsureSuccessStatusCode();
        }

        public async Task<DocumentInfo?> GetDocumentDetailsAsync(Guid subscriptionId, Guid ContainerId, CancellationToken token = default)
        {
            using var response = await _httpClient.GetAsync($"/dms/api/{subscriptionId}/documents/containers/{ContainerId}/details", token);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<DocumentInfo>(token);
        }

        public async Task PurgeRecycleBinItemsAsync(Guid subscriptionId, RecycleBinItem model, CancellationToken token = default)
        {
            string url = $"/dms/api/{subscriptionId}/documents/recyclebin/purge";
            var response = await _httpClient.PostAsJsonAsync(url, model, token);
            response.EnsureSuccessStatusCode();
        }

        public async Task<QuerySet<DocumentInfo>> QueryContentAsync(Guid subscriptionId, Guid? ContainerId = null, int skip = 0, int take = 10, string search = "", string? orderBy = null, bool descending = false, CancellationToken cancellationToken = default)
        {
            logger.LogDebug("Querying documents in subscription {SubscriptionId} ContainerId: {ContainerId} Skip: {Skip} Take: {Take} Search: {Search} OrderBy: {OrderBy} Descending: {Descending}",
                subscriptionId, ContainerId, skip, take, search, orderBy, descending);

            string url = ContainerId.HasValue ? $"/dms/api/{subscriptionId}/documents/containers/{ContainerId}" : $"/dms/api/{subscriptionId}/documents";

            string queryString = BuildQueryString(skip, take, orderBy, descending, search);

            if (!string.IsNullOrEmpty(queryString))
            {
                url += $"?{queryString}";
            }

            var response = await _httpClient.GetAsync(url, cancellationToken);
            try
            {
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadFromJsonAsync<QuerySet<DocumentInfo>>(cancellationToken) ?? new QuerySet<DocumentInfo>();
                logger.LogDebug("Query returned {TotalCount} items", result.TotalCount);
                return result;
            }
            catch (Exception ex)
            {
               
                throw;
            }

        }

        public async Task<QuerySet<DocumentInfo>> QueryDeletedAsync(Guid subscriptionId, int skip = 0, int take = 10, string search = "", string? orderBy = null, bool descending = false, CancellationToken cancellationToken = default)
        {
            string url = $"/dms/api/{subscriptionId}/documents/recyclebin";

            string queryString = BuildQueryString(skip, take, orderBy, descending, search);

            if (!string.IsNullOrEmpty(queryString))
            {
                url += $"?{queryString}";
            }

            var response = await _httpClient.GetFromJsonAsync<QuerySet<DocumentInfo>>(url, cancellationToken) ?? new QuerySet<DocumentInfo>();
            return response;
        }

        public async Task RestoreAsync(Guid subscriptionId, RecycleBinItem model, CancellationToken token = default)
        {
            var response = await _httpClient.PostAsJsonAsync($"/dms/api/{subscriptionId}/documents/recyclebin/restore", model, token);
            response.EnsureSuccessStatusCode();
        }

        private static string BuildQueryString(int skip, int take, string? orderBy, bool descending, string search = "")
        {
            var filters = new Dictionary<string, string>
            {
                ["skip"] = skip.ToString(),
                ["take"] = take.ToString(),
                ["orderBy"] = orderBy ?? "name",
                ["descending"] = descending.ToString()
            };

            if (!string.IsNullOrEmpty(search))
            {
                filters["search"] = search;
            }

            return string.Join("&", filters.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
        }
    }
}
