using DocumentManagementService.Data;
using DocumentManagementService.Domain;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Common.Enums;
using Shared.Common.Interfaces;
using Shared.Models;
using System.Linq;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = null; //unlimited
});

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = long.MaxValue;
});

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
});

builder.Services.AddDbContextPool<ApplicationDbContext>(o =>
{
    o.UseSqlServer(builder.Configuration.GetConnectionString("DmsDbConnection"));
});
builder.Services.AddScoped<IDocumentService, DocumentManagementService.Services.DocumentService>();
builder.Services.AddScoped<ISharingService, DocumentManagementService.Services.SharingService>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(builder =>
        builder.Expire(TimeSpan.FromSeconds(10)));
});

//builder.Services.AddHostedService<DocumentManagementService.Services.CleanUpService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
//app.UseOutputCache();
app.UseAntiforgery();

app.Use(async (ctx, next) =>
{
    var subscription = ctx.GetRouteValue("subscriptionId");
    if (string.IsNullOrWhiteSpace(subscription?.ToString()) || !Guid.TryParse(subscription.ToString(), out Guid subscriptionId) || subscriptionId == Guid.Empty)
    {
        ctx.Response.StatusCode = StatusCodes.Status400BadRequest;
        await ctx.Response.WriteAsync("Invalid subscription id.");
        return;
    }

    var user = ctx.GetRouteValue("userId");
    if (string.IsNullOrWhiteSpace(user?.ToString()) || !Guid.TryParse(user.ToString(), out Guid userId) || userId == Guid.Empty)
    {
        ctx.Response.StatusCode = StatusCodes.Status400BadRequest;
        await ctx.Response.WriteAsync("Invalid user id.");
        return;
    }

    await next();
});

var documentApis = app.MapGroup("/api/users/{userId}/subscriptions/{subscriptionId}/documents");

#region Document APIs

/// Get root contents
documentApis.MapGet("/", async ([FromServices] ApplicationDbContext dbContext, [FromServices] IDocumentService documentService, Guid subscriptionId, Guid userId, HttpRequest request, [FromQuery] bool descending = false, [FromQuery] string? search = null, [FromQuery] string orderBy = nameof(DocumentInfo.Name), [FromQuery] int skip = 0, [FromQuery] int take = 25, CancellationToken cancellationToken = default) =>
{
    try
    {
        if (skip < 0) skip = 0;
        if (take <= 0) take = 10;

        if (take > 100) take = 100;

        // var userName = request.HttpContext?.User.Identity?.Name;

        var result = await documentService.QueryContentAsync(userId, subscriptionId, null, skip, take, search, orderBy, descending, cancellationToken);
        return Results.Ok(result);
    }
    catch (Exception)
    {
        return Results.Problem("Error occurred while retrieving content.");
    }
}).Produces<QuerySet<Shared.Models.DocumentInfo>>();

/// Get Container contents
documentApis.MapGet("/containers/{id}", async ([FromServices] IDocumentService documentService, Guid subscriptionId, Guid id, Guid userId, HttpRequest request, [FromQuery] bool descending = false, [FromQuery] string? search = null, [FromQuery] string OrderBy = nameof(DocumentInfo.Name), [FromQuery] int skip = 0, [FromQuery] int take = 10, CancellationToken cancellationToken = default) =>
{
    if (skip < 0) skip = 0;
    if (take <= 0) take = 10;

    var results = await documentService.QueryContentAsync(userId, subscriptionId, id, skip, take, search, OrderBy, descending, cancellationToken);

    return Results.Ok(results);

}).Produces<QuerySet<DocumentInfo>>();

/// Get Container details
documentApis.MapGet("/containers/{id}/details", async ([FromServices] IDocumentService documentService, Guid subscriptionId, Guid userId, Guid id) =>
{
    var container = await documentService.GetDocumentDetailsAsync(userId, subscriptionId, id);

    if (container == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(container);

}).Produces<DocumentInfo>();

/// Get recycle bin contents
documentApis.MapGet("/recyclebin", async ([FromServices] IDocumentService documentService, Guid subscriptionId, Guid userId, HttpRequest request, [FromQuery] bool descending = false, [FromQuery] string? search = null, [FromQuery] string OrderBy = nameof(DocumentInfo.Name), [FromQuery] int skip = 0, [FromQuery] int take = 25, CancellationToken cancellationToken = default) =>
{
    if (skip < 0) skip = 0;
    if (take <= 0) take = 10;

    if (take > 100) take = 100;

    var userName = request.HttpContext?.User.Identity?.Name;

    var results = await documentService.QueryDeletedAsync(userId, subscriptionId, skip, take, search ?? string.Empty, OrderBy, descending, cancellationToken);

    return Results.Ok(results);

}).Produces<QuerySet<Shared.Models.DocumentInfo>>();


//Soft delete item
documentApis.MapDelete("/{id}", async ([FromServices] IDocumentService documentService, Guid subscriptionId, Guid userId, Guid id, CancellationToken cancellationToken = default) =>
{
    if (id == Guid.Empty)
    {
        return Results.ValidationProblem(new Dictionary<string, string[]> { { "id", new[] { "Container id is required." } } });
    }

    if (subscriptionId == Guid.Empty)
    {
        return Results.ValidationProblem(new Dictionary<string, string[]> { { "subscriptionId", new[] { "Subscription id is required." } } });
    }

    await documentService.DeleteAsync(userId, subscriptionId, id, cancellationToken);

    return Results.Ok();
});

/// Create a new Container
documentApis.MapPost("/containers", async ([FromServices] ApplicationDbContext dbContext, [FromServices] IDocumentService documentService, Guid subscriptionId, Guid userId, CreateContainer request, CancellationToken cancellationToken, ILogger<Program> logger) =>
{
    if (string.IsNullOrWhiteSpace(request.Name))
    {
        return Results.BadRequest("Container name is required.");
    }
    try
    {
        var found = await dbContext.Containers.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Name == request.Name.Trim() && x.UserId == userId && x.SubscriptionId == subscriptionId && (x.ParentId == null || (request.ParentId.HasValue && x.ParentId == request.ParentId.Value)));

        if (found != null)
        {
            if (found.IsDeleted)
            {
                return Results.Problem("Cannot create a new {{Container}}+ with the same name as a deleted one.", statusCode: StatusCodes.Status410Gone);
            }
            return Results.Problem("A Container with the same name already exists.", statusCode: StatusCodes.Status409Conflict);
        }

        return Results.Ok(await documentService.CreateContainerAsync(userId, subscriptionId, request, cancellationToken));
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error creating container for subscription {SubscriptionId}", subscriptionId);
        return Results.Problem("An error occurred while creating the Container.");
    }
}).Produces<DocumentInfo>()
    .ProducesProblem(StatusCodes.Status500InternalServerError);

documentApis.MapPost("/recyclebin/empty", async ([FromServices] IDocumentService documentService, Guid subscriptionId, Guid userId, ILogger<Program> logger) =>
{
    if (subscriptionId == Guid.Empty)
    {
        return Results.ValidationProblem(new Dictionary<string, string[]> { { "subscriptionId", new[] { "Subscription id is required." } } });
    }

    try
    {
        await documentService.EmptyRecycleBinAsync(userId, subscriptionId);
        return Results.Ok();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error emptying recycle bin for subscription {SubscriptionId}", subscriptionId);
        return Results.Problem("An error occurred while emptying the recycle bin.");
    }

});

documentApis.MapPost("/recyclebin/restore", async ([FromServices] IDocumentService documentService, [FromRoute] Guid subscriptionId, [FromRoute] Guid userId, [FromBody] RecycleBinItem model) =>
{
    if (model.Items == null || !model.Items.Any())
    {
        return Results.BadRequest("No items specified for restoration.");
    }

    await documentService.RestoreAsync(userId, subscriptionId, model);

    return Results.Ok();
});

documentApis.MapPost("/recyclebin/purge", async ([FromServices] IDocumentService documentService, Guid subscriptionId, [FromRoute] Guid userId, [FromBody] RecycleBinItem model, ILogger<Program> logger) =>
{
    if (model.Items == null || !model.Items.Any())
    {
        return Results.ValidationProblem(new Dictionary<string, string[]> { { "items", new[] { "No items specified for deletion." } } });
    }

    await documentService.PurgeRecycleBinItemsAsync(userId, subscriptionId, model);

    return Results.Ok();
});

documentApis.MapPost("/upload/stream", async ([FromServices] ApplicationDbContext dbContext, [FromForm] IFormFile file, Guid subscriptionId, [FromRoute] Guid userId, [FromForm] string container) =>
{

    var fileName = Uri.UnescapeDataString(file.FileName);

    var friendlyName = Path.GetFileNameWithoutExtension(fileName).Trim();
    var extension = Path.GetExtension(fileName);
    int index = 0;

    var query = dbContext.Documents.Where(x => x.SubscriptionId == subscriptionId && x.UserId == userId).AsQueryable();

    if (Guid.TryParse(container, out var containerId))
    {
        query = query.Where(x => x.ContainerId == containerId);
    }

    while (await query.Where(x => x.Name == friendlyName).AnyAsync())
    {
        friendlyName = $"{Path.GetFileNameWithoutExtension(fileName)} ({++index})";
    }

    var d = Directory.CreateDirectory("c:\\temp\\uploads");
    var name = $"{Guid.NewGuid()}{extension}";
    var path = Path.Combine(d.FullName, name);

    await using var fs = new FileStream(
        path,
        FileMode.Create,
        FileAccess.Write,
        FileShare.None,
        bufferSize: 1024 * 1024,
        useAsync: true);
    app.Logger.LogError("Starting file upload: {FileName} to {Path}", fileName, path);
    app.Logger.LogError("Request Body Length: {Length}", file.Length);

    await file.CopyToAsync(fs);

    await dbContext.Documents.AddAsync(new LynkDocument(friendlyName)
    {
        Id = Guid.NewGuid(),
        ContainerId = containerId == Guid.Empty ? null : containerId,
        Location = Path.Combine(d.FullName, name),
        SubscriptionId = subscriptionId,
        ModifiedOn = DateTime.UtcNow,
        Type = file.ContentType,
        UserId = userId,
        Extension = extension
    });

    await dbContext.SaveChangesAsync();

}).DisableAntiforgery();
#endregion

#region Shares logic APIs
documentApis.MapPost("/shares", async ([FromServices] ApplicationDbContext dbContext, [FromRoute] Guid userId, [FromBody] ShareRequest model, HttpRequest request) =>
{

    if (model.ReferenceId == null || model.ReferenceId == Guid.Empty)
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            { nameof(model.ReferenceId), new[] { "ReferenceId is required." } }
        });
    }

    dbContext.Shares.Add(new LynkShare
    {
        Id = Guid.NewGuid(),
        ReferenceId = model.ReferenceId.Value
    });

    await dbContext.SaveChangesAsync();

    return Results.Ok();
});

documentApis.MapDelete("/shares/{id}", async ([FromServices] ApplicationDbContext dbContext, Guid id, [FromRoute] Guid userId, HttpRequest request) =>
{
    var share = await dbContext.Shares.FirstOrDefaultAsync(s => s.Id == id);
    if (share == null)
    {
        return Results.NotFound();
    }
    dbContext.Shares.Remove(share);
    await dbContext.SaveChangesAsync();
    return Results.Ok();
});

documentApis.MapPut("/shares/{id}", async ([FromServices] ApplicationDbContext dbContext, Guid id, [FromRoute] Guid userId, [FromBody] ShareRequest model, HttpRequest request) =>
{
    if (model.ReferenceId == null || model.ReferenceId == Guid.Empty)
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            { nameof(model.ReferenceId), new[] { "ReferenceId is required." } }
        });
    }

    var share = await dbContext.Shares.FirstOrDefaultAsync(s => s.Id == id);

    if (share == null)
    {
        return Results.NotFound();
    }

    share.ReferenceId = model.ReferenceId.Value;
    await dbContext.SaveChangesAsync();
    return Results.Ok();
});

documentApis.MapGet("/shares", async ([FromServices] ISharingService sharingService, [FromRoute] Guid subscriptionId, [FromRoute] Guid userId, [FromQuery] bool descending = false, [FromQuery] string? search = null, [FromQuery] string OrderBy = nameof(ShareRequest.Name), [FromQuery] int skip = 0, [FromQuery] int take = 25, CancellationToken cancellationToken = default) =>
{
    var result = await sharingService.QueryAsync(userId, subscriptionId, skip, take, OrderBy, descending, cancellationToken);
    return Results.Ok(result);
});
#endregion

#region Pinned Apis
documentApis.MapGet("/pinned", async ([FromServices] IDocumentService documentService, [FromRoute] Guid subscriptionId, [FromRoute] Guid userId) =>
{
    var result = await documentService.GetPinnedDocumentsAsync(userId, subscriptionId);
    return Results.Ok(result);
});

documentApis.MapPost("/pin", async ([FromServices] IDocumentService documentService, Guid subscriptionId, [FromRoute] Guid userId, [FromBody] PinRequest model) =>
{
    await documentService.PinAsync(userId, subscriptionId, model);
    return Results.Ok();
});

documentApis.MapPost("/unpin", async ([FromServices] IDocumentService documentService, Guid subscriptionId, [FromRoute] Guid userId, [FromBody] PinRequest model) =>
{
    await documentService.UnpinAsync(userId, subscriptionId, model);
    return Results.Ok();
});
#endregion

app.Run();