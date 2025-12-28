
using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace DMS.Web.Authentication
{
    public class CookieMessageHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // include cookies!
            request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
            request.Headers.Add("X-Requested-With", ["XMLHttpRequest"]);
            return base.SendAsync(request, cancellationToken);
        }
    }
}
