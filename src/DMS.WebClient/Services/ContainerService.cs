using DMS.WebClient.Models;
using MudBlazor;
using Shared.Common.Interfaces;
using Shared.Models;
using Shared.Requests;
using System;
using System.Net.Http;
using System.Net.Http.Json;

namespace DMS.WebClient.Services
{
    public class ContainerService(IHttpClientFactory factory) : IDocumentService
    {
        HttpClient _httpClient => factory.CreateClient("backend");

        public async Task<DocumentInfo?> CreateContainerAsync(Guid subscriptionId, CreateContainer Container)
        {
            using var response = await _httpClient.PostAsJsonAsync($"/dms/api/{subscriptionId}/documents/containers", Container);
            try
            {
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<DocumentInfo>();
            }
            catch (HttpRequestException hre)
            {
                var content = await response.Content.ReadFromJsonAsync<ProblemDetails>();

                throw new ApplicationException(content?.Detail ?? hre.Message, hre);
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

        public async Task<DocumentInfo?> GetDetailsAsync(Guid subscriptionId, Guid ContainerId)
        {
            using var response = await _httpClient.GetAsync($"/dms/api/{subscriptionId}/documents/containers/{ContainerId}/details");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<DocumentInfo>();
        }

        public async Task<QuerySet<DocumentInfo>> QueryAsync(Guid subscriptionId, Guid? ContainerId = null, int skip = 0, int take = 10, string search = "", string? orderBy = null, bool descending = false, CancellationToken cancellationToken = default)
        {

            string url = ContainerId.HasValue ? $"/dms/api/{subscriptionId}/documents/containers/{ContainerId}" : $"/dms/api/{subscriptionId}/documents";

            var filters = new Dictionary<string, string>();
            filters["skip"] = skip.ToString();
            filters["take"] = take.ToString();
            filters["orderBy"] = orderBy ?? "name";
            filters["descending"] = descending.ToString();

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

            var response = await _httpClient.GetFromJsonAsync<QuerySet<DocumentInfo>>(url, cancellationToken) ?? new QuerySet<DocumentInfo>();
            return response;
        }

        public async Task<QuerySet<DocumentInfo>> QueryDeletedAsync(Guid subscriptionId, int skip = 0, int take = 10, string search = "", string? orderBy = null, bool descending = false, CancellationToken cancellationToken = default)
        {

            string url = $"/dms/api/{subscriptionId}/documents/recyclebin";

            var filters = new Dictionary<string, string>();
            filters["skip"] = skip.ToString();
            filters["take"] = take.ToString();
            filters["orderBy"] = orderBy ?? "name";
            filters["descending"] = descending.ToString();

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

            var response = await _httpClient.GetFromJsonAsync<QuerySet<DocumentInfo>>(url, cancellationToken) ?? new QuerySet<DocumentInfo>();
            return response;
        }

        public async Task RestoreAsync(Guid subscriptionId, Guid id)
        {
            var response = await _httpClient.PostAsync($"/dms/api/{subscriptionId}/documents/recyclebin/restore/{id}", null);
            response.EnsureSuccessStatusCode();
        }

    }
}
