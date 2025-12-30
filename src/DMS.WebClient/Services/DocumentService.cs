using DMS.WebClient.Models;
using Shared.Common.Interfaces;
using Shared.Models;
using System.Net.Http.Json;

namespace DMS.WebClient.Services
{
    public class DocumentService(IHttpClientFactory factory, ILogger<DocumentService> logger) : IDocumentService
    {
        readonly HttpClient _httpClient = factory.CreateClient("backend");

        public async Task<DocumentInfo?> CreateContainerAsync(Guid subscriptionId, CreateContainer Container)
        {
            logger.LogDebug("Creating container {ContainerName}", Container.Name);

            using var response = await _httpClient.PostAsJsonAsync($"/dms/api/{subscriptionId}/documents/containers", Container);
            try
            {
                response.EnsureSuccessStatusCode();
                logger.LogDebug("Container {ContainerName} created successfully", Container.Name);
                return await response.Content.ReadFromJsonAsync<DocumentInfo>();
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

        public async Task DeleteAsync(Guid subscriptionId, Guid id)
        {
            using var response = await _httpClient.DeleteAsync($"/dms/api/{subscriptionId}/documents/{id}");
            response.EnsureSuccessStatusCode();
        }

        public async Task EmptyRecycleBinAsync(Guid subscriptionId)
        {
            using var response = await _httpClient.PostAsync($"/dms/api/{subscriptionId}/documents/recyclebin/empty", null);
            response.EnsureSuccessStatusCode();
        }

        public async Task<DocumentInfo?> GetDetailsAsync(Guid subscriptionId, Guid ContainerId)
        {
            using var response = await _httpClient.GetAsync($"/dms/api/{subscriptionId}/documents/containers/{ContainerId}/details");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<DocumentInfo>();
        }

        public async Task PurgeRecycleBinItemsAsync(Guid subscriptionId, RecycleBinItem model)
        {
            string url = $"/dms/api/{subscriptionId}/documents/recyclebin/purge";
            var response = await _httpClient.PostAsJsonAsync(url, model);
            response.EnsureSuccessStatusCode();
        }

        public async Task<QuerySet<DocumentInfo>> QueryAsync(Guid subscriptionId, Guid? ContainerId = null, int skip = 0, int take = 10, string search = "", string? orderBy = null, bool descending = false, CancellationToken cancellationToken = default)
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

        public async Task RestoreAsync(Guid subscriptionId, RecycleBinItem model)
        {
            var response = await _httpClient.PostAsJsonAsync($"/dms/api/{subscriptionId}/documents/recyclebin/restore", model);
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
