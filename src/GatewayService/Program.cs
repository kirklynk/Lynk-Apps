using GatewayService.Security;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Threading.RateLimiting;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<SecurityDbContext>(context =>
{
    context.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});
//builder.Services.AddAuthentication(c =>
//{
//    c.DefaultAuthenticateScheme = "Cookies";
//    c.DefaultChallengeScheme = "Cookies";
//}).AddCookie("Cookies", c =>
//{
//    c.Cookie.Name = builder.Configuration.GetValue<string>("cookies:name");
//    c.DataProtectionProvider = DataProtectionProvider.Create(builder.Configuration.GetValue<string>("cookies:provider") ?? "lynk_services_cookies");
//});

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

builder.Services.AddIdentityApiEndpoints<User>(options =>
{

}).AddEntityFrameworkStores<SecurityDbContext>().AddApiEndpoints();

// YARP: add the reverse proxy services
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(context =>
    {
        // Mark requests as coming from the trusted gateway
        context.AddRequestTransform(ctx =>
        {
            ctx.ProxyRequest.Headers.Add("X-Gateway-Auth", "true");

            var user = ctx.HttpContext.User;

            if (user.Identity?.IsAuthenticated == true)
            {
                ctx.ProxyRequest.Headers.Add(
                    "X-User-Name", user.Identity.Name);
            }

            return ValueTask.CompletedTask;
        });
    });

builder.Services.AddAuthorizationBuilder().AddPolicy("Dms", policy =>
{
    //policy.RequireRole("Admin");
    policy.RequireAuthenticatedUser();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("CorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapIdentityApi<User>();

app.MapGet("/app/user/info", async (HttpRequest request, [FromServices] UserManager<User> userManager, [FromServices] SecurityDbContext dbContext) =>
{

    var user = request?.HttpContext?.User?.Identity?.Name;
    var appId = request?.Headers["x-application-id"].FirstOrDefault();

    if (user == null)
        return Results.BadRequest();

    var userEntity = await userManager.FindByNameAsync(user) ?? throw new NotFoundException();
    var userLogins = await dbContext.UserClaims.Where(uc => uc.UserId == userEntity.Id).ToListAsync();

    var subscriptions = await dbContext.Subscriptions.Include(x => x.Users)
        .Where(s => s.Users.Any(u => u.Id == userEntity.Id))
        .Select(s => new { s.Id, s.Name })
        .ToListAsync();

    return Results.Ok(new { userEntity.Email, Subscriptions = subscriptions, userEntity.FullName });

}).RequireAuthorization();

app.MapPost("/logout", async (HttpRequest request, [FromServices] SignInManager<User> signInManager) =>
{
    await signInManager.SignOutAsync();
    return Results.Ok();
});

app.UseRateLimiter();

// YARP: enable the reverse proxy endpoints
app.MapReverseProxy();

app.Run();

public class NotFoundException : Exception
{
}
