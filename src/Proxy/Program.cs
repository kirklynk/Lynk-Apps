using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Proxy.Data;
using Shared.Models;
using System.Security;
using System.Security.Claims;
using System.Threading.RateLimiting;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.WebHost.ConfigureKestrel(options=>
{
    options.Limits.MaxRequestBodySize = null; //unlimited
});

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = int.MaxValue; 
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});

builder.Services.AddDefaultIdentity<User>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager<SignInManager<User>>()
    .AddUserManager<UserManager<User>>();

// YARP: add the reverse proxy services
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
    //.AddTransforms(context =>
    //{
    //    // Mark requests as coming from the trusted gateway
    //    context.AddRequestTransform(ctx =>
    //    {
    //        ctx.ProxyRequest.Headers.Add("X-Gateway-Auth", "true");
    //        return ValueTask.CompletedTask;
    //    });
    //});

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("Dms", policy =>
{
    //policy.RequireRole("Admin");
    policy.RequireAuthenticatedUser();
});

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("dms-rate-limiter", config =>
    {
        config.PermitLimit = 10;
        config.Window = TimeSpan.FromMinutes(1);
        config.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        config.QueueLimit = 2;
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.WithOrigins("https://localhost:7282")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddRateLimiter();

builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseCors("CorsPolicy");
app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.UseRateLimiter();

// YARP: enable the reverse proxy endpoints
app.MapReverseProxy();


app.MapPost("/login", async Task<Results<Ok<AccessTokenResponse>, EmptyHttpResult, ProblemHttpResult>>
            ([FromBody] LoginRequest login, [FromQuery] bool? useCookies, [FromQuery] bool? useSessionCookies, [FromServices] IServiceProvider sp) =>
{
    var signInManager = sp.GetRequiredService<SignInManager<User>>();

    var useCookieScheme = (useCookies == true) || (useSessionCookies == true);
    var isPersistent = (useCookies == true) && (useSessionCookies != true);

    signInManager.AuthenticationScheme = useCookieScheme ? IdentityConstants.ApplicationScheme : IdentityConstants.BearerScheme;

    var result = await signInManager.PasswordSignInAsync(login.Email, login.Password, isPersistent, lockoutOnFailure: true);

    if (result.RequiresTwoFactor)
    {
        if (!string.IsNullOrEmpty(login.TwoFactorCode))
        {
            result = await signInManager.TwoFactorAuthenticatorSignInAsync(login.TwoFactorCode, isPersistent, rememberClient: isPersistent);
        }
        else if (!string.IsNullOrEmpty(login.TwoFactorRecoveryCode))
        {
            result = await signInManager.TwoFactorRecoveryCodeSignInAsync(login.TwoFactorRecoveryCode);
        }
    }

    if (!result.Succeeded)
    {
        return TypedResults.Problem(result.ToString(), statusCode: StatusCodes.Status401Unauthorized);
    }

    // The signInManager already produced the needed response in the form of a cookie or bearer token.
    return TypedResults.Empty;
});

app.MapPost("/logout", async (HttpRequest request, [FromServices] SignInManager<User> signInManager) =>
{
    await signInManager.SignOutAsync();
    return Results.Ok();
}).RequireAuthorization();

app.MapGet("/user/info", async (HttpRequest request, [FromServices] UserManager<User> userManager, [FromServices] ApplicationDbContext dbContext) =>
{

    var user = request?.HttpContext?.User?.Identity?.Name;
    var appId = request?.Headers["x-application-id"].FirstOrDefault();

    if (user == null)
        return Results.BadRequest();

    var userEntity = await userManager.FindByNameAsync(user);
    var userLogins = await dbContext.UserClaims.Where(uc => uc.UserId == userEntity.Id).ToListAsync();

    var subscriptions = await dbContext.Subscriptions.Include(x => x.Users)
        .Where(s => s.Users.Any(u => u.Id == userEntity.Id))
        .Select(s => new { s.Id, s.Name })
        .ToListAsync();

    return Results.Ok(new { userEntity.Email, Subscriptions = subscriptions, userEntity.FullName, userEntity.Id });

}).RequireAuthorization();

/* Refresh token endpoint 
app.MapPost("/refresh", async Task<Results<Ok<AccessTokenResponse>, UnauthorizedHttpResult, SignInHttpResult, ChallengeHttpResult>>
            ([FromBody] RefreshRequest refreshRequest, [FromServices] IServiceProvider sp) =>
 {
     var signInManager = sp.GetRequiredService<SignInManager<User>>();
     var refreshTokenProtector = bearerTokenOptions.Get(IdentityConstants.BearerScheme).RefreshTokenProtector;
     var refreshTicket = refreshTokenProtector.Unprotect(refreshRequest.RefreshToken);

     // Reject the /refresh attempt with a 401 if the token expired or the security stamp validation fails
     if (refreshTicket?.Properties?.ExpiresUtc is not { } expiresUtc ||
         timeProvider.GetUtcNow() >= expiresUtc ||
         await signInManager.ValidateSecurityStampAsync(refreshTicket.Principal) is not User user)

     {
         return TypedResults.Challenge();
     }

     var newPrincipal = await signInManager.CreateUserPrincipalAsync(user);
     return TypedResults.SignIn(newPrincipal, authenticationScheme: IdentityConstants.BearerScheme);
 });
*/
app.Run();
