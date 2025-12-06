using DMS.WebClient.Models;
using DMS.WebClient.Pages;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;

namespace DMS.WebClient.Authentication
{
    public class CookieAuthenticationStateProvider(IHttpClientFactory clientFactory, ILogger<CookieAuthenticationStateProvider> logger) : AuthenticationStateProvider, IAccountManagement
    {
        private readonly HttpClient httpClient = clientFactory.CreateClient("backend");
        bool _IsAuthenticated = false;
        private readonly ClaimsPrincipal _Unauthenticated = new(new ClaimsIdentity());
        private readonly JsonSerializerOptions jsonSerializerOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            _IsAuthenticated = false;

            // default to not authenticated
            var user = _Unauthenticated;

            try
            {
                // the user info endpoint is secured, so if the user isn't logged in this will fail
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("x-application-id", $"{Guid.NewGuid()}");

                var userInfo = await httpClient.GetFromJsonAsync<UserInfo>("/app/user/info");

                if (userInfo != null)
                {
                    // in this example app, name and email are the same
                    var claims = new List<Claim>
                    {
                        new(ClaimTypes.Name, userInfo.Email),
                        new(ClaimTypes.Email, userInfo.Email),
                    };

                    // add any additional claims
                    //claims.AddRange(
                    //    userInfo.Claims.Where(c => c.Key != ClaimTypes.Name && c.Key != ClaimTypes.Email)
                    //        .Select(c => new Claim(c.Key, c.Value)));

                    //// request the roles endpoint for the user's roles
                    //using var rolesResponse = await httpClient.GetAsync("roles");

                    //// throw if request fails
                    //rolesResponse.EnsureSuccessStatusCode();

                    //// read the response into a string
                    //var rolesJson = await rolesResponse.Content.ReadAsStringAsync();

                    //// deserialize the roles string into an array
                    //var roles = JsonSerializer.Deserialize<RoleClaim[]>(rolesJson, jsonSerializerOptions);

                    //// add any roles to the claims collection
                    //if (roles?.Length > 0)
                    //{
                    //    foreach (var role in roles)
                    //    {
                    //        if (!string.IsNullOrEmpty(role.Type) && !string.IsNullOrEmpty(role.Value))
                    //        {
                    //            claims.Add(new Claim(role.Type, role.Value, role.ValueType, role.Issuer, role.OriginalIssuer));
                    //        }
                    //    }
                    //}

                    // set the principal
                    var id = new ClaimsIdentity(claims, nameof(CookieAuthenticationStateProvider));
                    user = new ClaimsPrincipal(id);
                    _IsAuthenticated = true;
                }
            }
            catch (Exception ex) when (ex is HttpRequestException exception)
            {
                if (exception.StatusCode != HttpStatusCode.Unauthorized)
                {
                    logger.LogError(ex, "App error");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "App error");
            }

            // return the state
            return new AuthenticationState(user);
        }

        public async Task<bool> LoginAsync(LoginRequest login)
        {
            try
            {
                var response = await httpClient.PostAsJsonAsync("/login?useCookies=true", login);
                NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<bool> LogoutAsync()
        {
            var response = await httpClient.PostAsJsonAsync("/logout", new { });
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            return response.IsSuccessStatusCode;
        }
    }
}
