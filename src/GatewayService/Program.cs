using GatewayService.Security;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<SecurityDbContext>(context =>
{
    context.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        return RateLimitPartition.GetFixedWindowLimiter("GlobalLimiter", _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 100,
            Window = TimeSpan.FromMinutes(1),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 2
        });
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
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

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

app.MapIdentityApi<User>();

app.UseAuthentication();
app.UseAuthorization();


app.MapGet("/app/user/info", async (HttpRequest request, [FromServices] UserManager<User> userManager, [FromServices] SecurityDbContext dbContext) =>
{

    var user = request?.HttpContext?.User?.Identity?.Name;
    var appId = request?.Headers["x-application-id"].FirstOrDefault();

    if (user == null)
        return Results.BadRequest();

    var userEntity = await userManager.FindByNameAsync(user) ?? throw new NotFoundException();
    var userLogins = await dbContext.UserClaims.Where(uc => uc.UserId == userEntity.Id).ToListAsync();

    var subscriptions = await dbContext.Users.Include(x => x.Subscriptions)
           .Where(x => x.Id == userEntity.Id)
           .SelectMany(x => x.Subscriptions).Select(x => new { x.Id, x.Name }).ToListAsync();

    return Results.Ok(new { userEntity.Email, Subscriptions = subscriptions });

}).RequireAuthorization();

app.MapPost("/logout", async (HttpRequest request, [FromServices] SignInManager<User> signInManager) =>
{
    await signInManager.SignOutAsync();
    return Results.Ok();
}).RequireAuthorization();


// YARP: enable the reverse proxy endpoints
app.MapReverseProxy();

app.Run();

public class NotFoundException : Exception
{
}