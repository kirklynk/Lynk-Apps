using DMS.WebClient.Models;
using Microsoft.JSInterop;
using Shared.Common.Interfaces;
using Shared.Models;
using System.Net.Http.Json;
using System.Reflection;

namespace DMS.WebClient.Services
{
    public class DocumentService(IHttpClientFactory factory, ILogger<DocumentService> logger) : IDocumentService
    {
        readonly HttpClient _httpClient = factory.CreateClient(Constants.DEFAULT_URL_KEY);

        public async Task<DocumentInfo?> CreateContainerAsync(Guid userId, Guid subscriptionId, CreateContainer Container, CancellationToken token = default)
        {
            logger.LogDebug("Creating container {ContainerName}", Container.Name);

            using var response = await _httpClient.PostAsJsonAsync($"/dms/api/users/{userId}/subscriptions/{subscriptionId}/documents/containers", Container, token);
            
            try
            {
                response.EnsureSuccessStatusCode();
                logger.LogDebug("Container {ContainerName} created successfully", Container.Name);
                return await response.Content.ReadFromJsonAsync<DocumentInfo>(token);
            }
            catch (HttpRequestException hre)
            {
                var content = await response.Content.ReadAsStringAsync();
                logger.LogError("Error creating container {ContainerName}: {ErrorDetail}", Container.Name, content ?? hre.Message);
                throw new ApplicationException(content ?? hre.Message, hre);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task DeleteAsync(Guid userId, Guid subscriptionId, Guid id, CancellationToken token = default)
        {
            using var response = await _httpClient.DeleteAsync($"/dms/api/users/{userId}/subscriptions/{subscriptionId}/documents/{id}", token);
            response.EnsureSuccessStatusCode();
        }

        public async Task EmptyRecycleBinAsync(Guid userId, Guid subscriptionId, CancellationToken token = default)
        {
            using var response = await _httpClient.PostAsync($"/dms/api/users/{userId}/subscriptions/{subscriptionId}/documents/recyclebin/empty", null, token);
            response.EnsureSuccessStatusCode();
        }

        public async Task<DocumentInfo?> GetDetailsAsync(Guid userId, Guid subscriptionId, Guid ContainerId, CancellationToken token = default)
        {
            using var response = await _httpClient.GetAsync($"/dms/api/users/{userId}/subscriptions/{subscriptionId}/documents/containers/{ContainerId}/details", token);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<DocumentInfo>(token);
        }

        public async Task PurgeRecycleBinItemsAsync(Guid userId, Guid           subscriptionId, RecycleBinItem model, CancellationToken token = default)
        {
            string url = $"/dms/api/users/{userId}/subscriptions/{subscriptionId}/documents/recyclebin/purge";
            var response = await _httpClient.PostAsJsonAsync(url, model, token);
            response.EnsureSuccessStatusCode();
        }

        public async Task<QuerySet<DocumentInfo>> QueryContentAsync(Guid userId, Guid subscriptionId, Guid? ContainerId = null, int skip = 0, int take = 10, string search = "", string? orderBy = null, bool descending = false, CancellationToken cancellationToken = default)
        {
            logger.LogDebug("Querying documents in subscription {SubscriptionId} ContainerId: {ContainerId} Skip: {Skip} Take: {Take} Search: {Search} OrderBy: {OrderBy} Descending: {Descending}",
                subscriptionId, ContainerId, skip, take, search, orderBy, descending);

            string url = ContainerId.HasValue ? $"/dms/api/users/{userId}/subscriptions/{subscriptionId}/documents/containers/{ContainerId}" : $"/dms/api/users/{userId}/subscriptions/{subscriptionId}/documents";

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

        public async Task<QuerySet<DocumentInfo>> QueryDeletedAsync(Guid userId, Guid subscriptionId, int skip = 0, int take = 10, string search = "", string? orderBy = null, bool descending = false, CancellationToken cancellationToken = default)
        {
            string url = $"/dms/api/users/{userId}/subscriptions/{subscriptionId}/documents/recyclebin";

            string queryString = BuildQueryString(skip, take, orderBy, descending, search);

            if (!string.IsNullOrEmpty(queryString))
            {
                url += $"?{queryString}";
            }

            var response = await _httpClient.GetFromJsonAsync<QuerySet<DocumentInfo>>(url, cancellationToken) ?? new QuerySet<DocumentInfo>();
            return response;
        }

        public async Task RestoreAsync(Guid userId, Guid subscriptionId, RecycleBinItem model, CancellationToken token = default)
        {
            var response = await _httpClient.PostAsJsonAsync($"/dms/api/users/{userId}/subscriptions/{subscriptionId}/documents/recyclebin/restore", model, token);
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

        public async Task PinAsync(Guid userId, Guid subscriptionId, PinRequest pin)
        {
            var response = await _httpClient.PostAsJsonAsync($"/dms/api/users/{userId}/subscriptions/{subscriptionId}/documents/pinned/add", pin);
            response.EnsureSuccessStatusCode();
        }

        public async Task UnpinAsync(Guid userId, Guid subscriptionId, List<PinRequest> pin)
        {
            var response = await _httpClient.PostAsJsonAsync($"/dms/api/users/{userId}/subscriptions/{subscriptionId}/documents/pinned/remove", pin);
            response.EnsureSuccessStatusCode();
        }

        public async Task<List<DocumentInfo>> GetPinnedItemsAsync(Guid userId, Guid subscriptionId, CancellationToken token = default)
        {
            var response = await _httpClient.GetAsync($"/dms/api/users/{userId}/subscriptions/{subscriptionId}/documents/pinned", token);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<DocumentInfo>>(token) ?? new List<DocumentInfo>();
        }
    }
}
