using Shared.Common.Interfaces;
using Shared.Models;
using System.Net.Http.Json;

namespace DMS.WebClient.Services
{
    public class SharingServices(IHttpClientFactory factory) : ISharingService
    {
        readonly HttpClient _httpClient = factory.CreateClient(Constants.DEFAULT_URL_KEY);

        public async Task<QuerySet<ShareRequest>> QueryAsync(Guid userId, Guid subscriptionId, int skip, int take, string? orderBy, bool descending, CancellationToken cancellationToken)
        {
            var url = $"/dms/api/users/{userId}/subscriptions/{subscriptionId}/documents/shares?skip={skip}&take={take}";
            if (!string.IsNullOrEmpty(orderBy))
            {
                url += $"&orderBy={orderBy}&descending={descending}";
            }
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<QuerySet<ShareRequest>>(cancellationToken: cancellationToken) ?? new QuerySet<ShareRequest>();

        }
    }
}
