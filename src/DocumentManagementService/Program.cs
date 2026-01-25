using DocumentManagementService.Data;
using DocumentManagementService.Domain;
using DocumentManagementService.Exceptions;
using DocumentManagementService.Services;
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
builder.Services.AddScoped<IApiDocumentService, DocumentManagementService.Services.DocumentService>();
builder.Services.AddScoped<ISharingService, DocumentManagementService.Services.SharingService>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddKeyedScoped<IFileStorageService, DocumentManagementService.Services.DiskFileStorageService>("DiskFileStorage");
builder.Services.AddKeyedScoped<IFileStorageService, DocumentManagementService.Services.AWSS3FileStorageService>("AWSS3FileStorage");
builder.Services.AddKeyedScoped<IFileStorageService, DocumentManagementService.Services.AzureBlobFileStorageService>("IAzureBlobFileStorage");

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
documentApis.MapGet("/", async ([FromServices] IApiDocumentService documentService, Guid subscriptionId, Guid userId, HttpRequest request, [FromQuery] bool descending = false, [FromQuery] string? search = null, [FromQuery] string orderBy = nameof(DocumentInfo.Name), [FromQuery] int skip = 0, [FromQuery] int take = 25, CancellationToken cancellationToken = default) =>
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
documentApis.MapGet("/containers/{id}", async ([FromServices] IApiDocumentService documentService, Guid subscriptionId, Guid id, Guid userId, HttpRequest request, [FromQuery] bool descending = false, [FromQuery] string? search = null, [FromQuery] string OrderBy = nameof(DocumentInfo.Name), [FromQuery] int skip = 0, [FromQuery] int take = 10, CancellationToken cancellationToken = default) =>
{
    if (skip < 0) skip = 0;
    if (take <= 0) take = 10;

    var results = await documentService.QueryContentAsync(userId, subscriptionId, id, skip, take, search, OrderBy, descending, cancellationToken);

    return Results.Ok(results);

}).Produces<QuerySet<DocumentInfo>>();

/// Get Container details
documentApis.MapGet("/containers/{id}/details", async ([FromServices] IApiDocumentService documentService, Guid subscriptionId, Guid userId, Guid id) =>
{
    var container = await documentService.GetDetailsAsync(userId, subscriptionId, id);

    if (container == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(container);

}).Produces<DocumentInfo>();

/// Get recycle bin contents
documentApis.MapGet("/recyclebin", async ([FromServices] IApiDocumentService documentService, Guid subscriptionId, Guid userId, HttpRequest request, [FromQuery] bool descending = false, [FromQuery] string? search = null, [FromQuery] string OrderBy = nameof(DocumentInfo.Name), [FromQuery] int skip = 0, [FromQuery] int take = 25, CancellationToken cancellationToken = default) =>
{
    if (skip < 0) skip = 0;
    if (take <= 0) take = 10;

    if (take > 100) take = 100;

    var userName = request.HttpContext?.User.Identity?.Name;

    var results = await documentService.QueryDeletedAsync(userId, subscriptionId, skip, take, search ?? string.Empty, OrderBy, descending, cancellationToken);

    return Results.Ok(results);

}).Produces<QuerySet<Shared.Models.DocumentInfo>>();


//Soft delete item
documentApis.MapDelete("/{id}", async ([FromServices] IApiDocumentService documentService, Guid subscriptionId, Guid userId, Guid id, CancellationToken cancellationToken = default) =>
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
documentApis.MapPost("/containers", async ([FromServices] IApiDocumentService documentService, Guid subscriptionId, Guid userId, CreateContainer request, CancellationToken cancellationToken, ILogger<Program> logger) =>
{
    if (string.IsNullOrWhiteSpace(request.Name))
    {
        return Results.BadRequest("Container name is required.");
    }

    try
    {
        return Results.Ok(await documentService.CreateContainerAsync(userId, subscriptionId, request, cancellationToken));
    }
    catch (ExistingException ex)
    {
        logger.LogError(ex, "Container creation failed due to existing container for subscription {SubscriptionId}", subscriptionId);
        return Results.Conflict("A container with the same name already exists.");
    }
    catch (DeletedException ex)
    {
        logger.LogError(ex, "Container creation failed due to deleted container for subscription {SubscriptionId}", subscriptionId);
        return Results.Conflict("A container with the same name was previously deleted. Please restore it from the recycle bin or choose a different name.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error creating container for subscription {SubscriptionId}", subscriptionId);
        return Results.Problem("An error occurred while creating the Container.");
    }
}).Produces<DocumentInfo>()
    .ProducesProblem(StatusCodes.Status500InternalServerError);

documentApis.MapPost("/recyclebin/empty", async ([FromServices] IApiDocumentService documentService, Guid subscriptionId, Guid userId, ILogger<Program> logger) =>
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

documentApis.MapPost("/recyclebin/restore", async ([FromServices] IApiDocumentService documentService, [FromRoute] Guid subscriptionId, [FromRoute] Guid userId, [FromBody] RecycleBinItem model) =>
{
    if (model.Items == null || !model.Items.Any())
    {
        return Results.BadRequest("No items specified for restoration.");
    }

    await documentService.RestoreAsync(userId, subscriptionId, model);

    return Results.Ok();
});

documentApis.MapPost("/recyclebin/purge", async ([FromServices] IApiDocumentService documentService, Guid subscriptionId, [FromRoute] Guid userId, [FromBody] RecycleBinItem model, ILogger<Program> logger) =>
{
    if (model.Items == null || !model.Items.Any())
    {
        return Results.ValidationProblem(new Dictionary<string, string[]> { { "items", new[] { "No items specified for deletion." } } });
    }

    await documentService.PurgeRecycleBinItemsAsync(userId, subscriptionId, model);

    return Results.Ok();
});

documentApis.MapPost("/upload/stream", async ([FromServices] IApiDocumentService documentService, [FromServices] IServiceProvider provider, [FromForm] IFormFile file, Guid subscriptionId, [FromRoute] Guid userId, [FromForm] string container, CancellationToken token = default) =>
{
    var p = provider.GetRequiredKeyedService<IFileStorageService>("DiskFileStorage");

    var fileName = Uri.UnescapeDataString(file.FileName);
    var fileNameNoExtension = Path.GetFileNameWithoutExtension(fileName).Trim();
    var extension = Path.GetExtension(fileName);

    var path = await p.SaveFileAsync(file, token);

    var document = await documentService.AddDocumentAsync(userId, subscriptionId, fileNameNoExtension, extension, container, path, file.ContentType, file.Length, token);

    return Results.Ok(document);

}).DisableAntiforgery();

documentApis.MapGet("/{id}/download", async ([FromServices] IApiDocumentService documentService, [FromServices] IServiceProvider provider, Guid subscriptionId, Guid userId, Guid id, CancellationToken token = default) =>
{
    var fileStorageService = provider.GetRequiredKeyedService<IFileStorageService>("DiskFileStorage");

    var doc = await documentService.GetAsync(userId, subscriptionId, id, token);
    if (doc == null)
    {
        return Results.NotFound();
    }
    Stream stream = await fileStorageService.GetStreamAsync(doc.Location, token);
    return Results.File(stream, doc.Type ?? "application/octet-stream", doc.Name + doc.Extension);
});
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
documentApis.MapGet("/pinned", async ([FromServices] IApiDocumentService documentService, [FromRoute] Guid subscriptionId, [FromRoute] Guid userId) =>
{

    var result = await documentService.GetPinnedItemsAsync(userId, subscriptionId);
    return Results.Ok(result);
});

documentApis.MapPost("/pinned/add", async ([FromServices] IApiDocumentService documentService, Guid subscriptionId, [FromRoute] Guid userId, [FromBody] PinRequest model) =>
{
    await documentService.PinAsync(userId, subscriptionId, model);
    return Results.Ok();
});

documentApis.MapPost("/pinned/remove", async ([FromServices] IApiDocumentService documentService, Guid subscriptionId, [FromRoute] Guid userId, [FromBody] List<PinRequest> model) =>
{
    await documentService.UnpinAsync(userId, subscriptionId, model);
    return Results.Ok();
});
#endregion

app.Run();