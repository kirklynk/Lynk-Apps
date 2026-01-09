using Shared.Common.Interfaces;
using Shared.Models;
using System.Net.Http.Json;

namespace DMS.WebClient.Services
{
    public class SharingServices(IHttpClientFactory factory) : ISharingService
    {
        readonly HttpClient _httpClient = factory.CreateClient(Constants.DEFAULT_URL_KEY);

        public async Task<QuerySet<ShareRequest>> QueryAsync(int skip, int take, string? orderBy, bool descending, CancellationToken cancellationToken)
        {
            var url = $"/dms/api/shares?skip={skip}&take={take}";
            if (!string.IsNullOrEmpty(orderBy))
            {
                url += $"&orderBy={orderBy}&descending={descending}";
            }
            return await _httpClient.GetFromJsonAsync<QuerySet<ShareRequest>>(url, cancellationToken) ?? new QuerySet<ShareRequest>();
        }
    }
}
