using DMS.Web;
using DMS.Web.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped<IAccountManagement, CookieAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider, CookieAuthenticationStateProvider>();
// register the cookie handler
builder.Services.AddTransient<CookieMessageHandler>();

builder.Services.AddHttpClient("backend", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["backend"] ?? throw new NotSupportedException());
}).AddHttpMessageHandler<CookieMessageHandler>();

builder.Services.AddMudServices();

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddAuthorizationCore();

await builder.Build().RunAsync();
