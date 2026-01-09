using DocumentManagementService.Data;
using DocumentManagementService.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Common.Enums;
using Shared.Common.Interfaces;
using Shared.Models;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


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

app.Use(async (ctx, next) =>
{
    if (!ctx.Request.Headers.TryGetValue("X-Gateway-Auth", out var value)
        || value != "true")
    {
        ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
        return;
    }

    await next();
});

#region Document APIs
var documentApis = app.MapGroup("/api/{subscriptionId}/documents");

/// Get root contents
documentApis.MapGet("/", async ([FromServices] ApplicationDbContext dbContext, [FromServices] IDocumentService documentService, Guid subscriptionId, HttpRequest request, [FromQuery] bool descending = false, [FromQuery] string? search = null, [FromQuery] string orderBy = nameof(DocumentInfo.Name), [FromQuery] int skip = 0, [FromQuery] int take = 25, CancellationToken cancellationToken = default) =>
{
    try
    {
        if (skip < 0) skip = 0;
        if (take <= 0) take = 10;

        if (take > 100) take = 100;

        // var userName = request.HttpContext?.User.Identity?.Name;

        var result = await documentService.QueryContentAsync(subscriptionId, null, skip, take, search, orderBy, descending, cancellationToken);
        return Results.Ok(result);
    }
    catch (Exception)
    {
        return Results.Problem("Error occurred while retrieving content.");
    }
}).Produces<QuerySet<Shared.Models.DocumentInfo>>();

/// Get Container contents
documentApis.MapGet("/containers/{id}", async ([FromServices] IDocumentService documentService, Guid subscriptionId, Guid id, HttpRequest request, [FromQuery] bool descending = false, [FromQuery] string? search = null, [FromQuery] string OrderBy = nameof(DocumentInfo.Name), [FromQuery] int skip = 0, [FromQuery] int take = 10, CancellationToken cancellationToken = default) =>
{
    try
    {

        if (skip < 0) skip = 0;
        if (take <= 0) take = 10;

        var results = await documentService.QueryContentAsync(subscriptionId, id, skip, take, search, OrderBy, descending, cancellationToken);

        return Results.Ok(results);
    }
    catch (Exception)
    {
        return Results.Problem("Error occurred while retrieving container contents.");
    }
}).Produces<QuerySet<DocumentInfo>>();

/// Get Container details
documentApis.MapGet("/containers/{id}/details", async ([FromServices] IDocumentService documentService, Guid subscriptionId, Guid id) =>
{
    var container = await documentService.GetDocumentDetailsAsync(subscriptionId, id);

    if (container == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(container);

}).Produces<DocumentInfo>();

/// Get recycle bin contents
documentApis.MapGet("/recyclebin", async ([FromServices] IDocumentService documentService, Guid subscriptionId, HttpRequest request, [FromQuery] bool descending = false, [FromQuery] string? search = null, [FromQuery] string OrderBy = nameof(DocumentInfo.Name), [FromQuery] int skip = 0, [FromQuery] int take = 25, CancellationToken cancellationToken = default) =>
{
    try
    {
        if (skip < 0) skip = 0;
        if (take <= 0) take = 10;

        if (take > 100) take = 100;

        var userName = request.HttpContext?.User.Identity?.Name;

        var results = await documentService.QueryDeletedAsync(subscriptionId, skip, take, search ?? string.Empty, OrderBy, descending, cancellationToken);

        return Results.Ok(results);
    }
    catch (Exception)
    {
        return Results.Problem("An error occurred while creating the Container.");
    }
}).Produces<QuerySet<Shared.Models.DocumentInfo>>();


//Soft delete item
documentApis.MapDelete("/{id}", async ([FromServices] IDocumentService documentService, Guid subscriptionId, Guid id, CancellationToken cancellationToken = default) =>
{
    if (id == Guid.Empty)
    {
        return Results.ValidationProblem(new Dictionary<string, string[]> { { "id", new[] { "Container id is required." } } });
    }



    if (subscriptionId == Guid.Empty)
    {
        return Results.ValidationProblem(new Dictionary<string, string[]> { { "subscriptionId", new[] { "Subscription id is required." } } });
    }

    await documentService.DeleteAsync(subscriptionId, id, cancellationToken);

    return Results.Ok();
});

/// Create a new Container
documentApis.MapPost("/containers", async ([FromServices] ApplicationDbContext dbContext, [FromServices] IDocumentService documentService, Guid subscriptionId, CreateContainer request, CancellationToken cancellationToken, ILogger<Program> logger) =>
{
    if (string.IsNullOrWhiteSpace(request.Name))
    {
        return Results.BadRequest("Container name is required.");
    }

    try
    {
        var found = await dbContext.Containers.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Name == request.Name.Trim() && x.SubscriptionId == subscriptionId && (request.ParentId.HasValue && x.ParentId == request.ParentId.Value));

        if (found != null)
        {
            if (found.IsDeleted)
            {
                return Results.Problem("Cannot create a new {{Container}}+ with the same name as a deleted one.", statusCode: StatusCodes.Status410Gone);
            }
            return Results.Problem("A Container with the same name already exists.", statusCode: StatusCodes.Status409Conflict);
        }

        return Results.Ok(await documentService.CreateContainerAsync(subscriptionId, request, cancellationToken));
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error creating container for subscription {SubscriptionId}", subscriptionId);
        return Results.Problem("An error occurred while creating the Container.");
    }
}).Produces<DocumentInfo>()
    .ProducesProblem(StatusCodes.Status500InternalServerError);

documentApis.MapPost("/recyclebin/empty", async ([FromServices] IDocumentService documentService, Guid subscriptionId, ILogger<Program> logger) =>
{
    if (subscriptionId == Guid.Empty)
    {
        return Results.ValidationProblem(new Dictionary<string, string[]> { { "subscriptionId", new[] { "Subscription id is required." } } });
    }

    try
    {
        await documentService.EmptyRecycleBinAsync(subscriptionId);
        return Results.Ok();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error emptying recycle bin for subscription {SubscriptionId}", subscriptionId);
        return Results.Problem("An error occurred while emptying the recycle bin.");
    }

});

documentApis.MapPost("/recyclebin/restore", async ([FromServices] ApplicationDbContext dbContext, [FromRoute] Guid subscriptionId, [FromBody] RecycleBinItem model, HttpRequest request) =>
{
    if (model.Items == null || !model.Items.Any())
    {
        return Results.BadRequest("No items specified for restoration.");
    }

    await dbContext.Containers.IgnoreQueryFilters()
        .Where(x => x.SubscriptionId == subscriptionId && x.IsDeleted && model.Items.Contains(x.Id))
        .ExecuteUpdateAsync(x =>
        {
            x.SetProperty(c => c.IsDeleted, false);
            x.SetProperty(c => c.DeletedOn, (DateTime?)null);
        });

    await dbContext.Documents.IgnoreQueryFilters()
        .Where(x => x.SubscriptionId == subscriptionId && x.IsDeleted && model.Items.Contains(x.Id))
        .ExecuteUpdateAsync(x =>
        {
            x.SetProperty(c => c.IsDeleted, false);
            x.SetProperty(c => c.DeletedOn, (DateTime?)null);
        });

    return Results.Ok();
});

documentApis.MapPost("/recyclebin/purge", async ([FromServices] ApplicationDbContext dbContext, Guid subscriptionId, [FromBody] RecycleBinItem model, ILogger<Program> logger) =>
{
    if (model.Items == null || !model.Items.Any())
    {
        return Results.ValidationProblem(new Dictionary<string, string[]> { { "items", new[] { "No items specified for deletion." } } });
    }

    try
    {
        var pendingPurge = dbContext.PendingPurge.IgnoreQueryFilters()
            .Where(x => x.SubscriptionId == subscriptionId)
            .Select(x => x.ReferenceId);

        var containers = dbContext.Containers.IgnoreQueryFilters().Where(x => x.SubscriptionId == subscriptionId && x.IsDeleted && !pendingPurge.Contains(x.Id) && model.Items.Contains(x.Id));

        if (await containers.AnyAsync())
        {
            await dbContext.PendingPurge.AddRangeAsync(containers.Select(c => new PendingPurge
            {
                ReferenceId = c.Id,
                SubscriptionId = subscriptionId,
                EntityType = EntityType.Container
            }));
            await dbContext.SaveChangesAsync();
        }

        var documents = dbContext.Documents.IgnoreQueryFilters().Where(x => x.SubscriptionId == subscriptionId && x.IsDeleted && !pendingPurge.Contains(x.Id) && model.Items.Contains(x.Id));

        if (await documents.AnyAsync())
        {
            await dbContext.PendingPurge.AddRangeAsync(documents.Select(d => new PendingPurge
            {
                ReferenceId = d.Id,
                SubscriptionId = subscriptionId,
                EntityType = EntityType.Document
            }));
            await dbContext.SaveChangesAsync();
        }
        return Results.Ok();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error emptying recycle bin for subscription {SubscriptionId}", subscriptionId);
        return Results.Problem("An error occurred while emptying the recycle bin.");
    }
});

documentApis.MapPost("/upload/stream", async ([FromServices] ApplicationDbContext dbContext, HttpRequest request, Guid subscriptionId, [FromHeader(Name = "X-ContainerId")] string? containerId) =>
{
    try
    {
        var fileNameHeader = request.Headers["X-File-Name"];
        var fileType = request.Headers["X-File-Type"];
        var fileName = Uri.UnescapeDataString(fileNameHeader.ToString());

        var friendlyName = Path.GetFileNameWithoutExtension(fileName).Trim();
        var extension = Path.GetExtension(fileName);
        int index = 0;
        var query = dbContext.Documents.Where(x => x.SubscriptionId == subscriptionId).AsQueryable();

        if (Guid.TryParse(containerId, out Guid containerGuid) && containerGuid != Guid.Empty)
        {
            query = query.Where(x => x.ContainerId == containerGuid);
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
        app.Logger.LogError("Request Body Length: {Length}", request.ContentLength);

        await request.Body.CopyToAsync(fs);

        await dbContext.Documents.AddAsync(new LynkDocument(friendlyName)
        {
            Id = Guid.NewGuid(),
            ContainerId = containerGuid != Guid.Empty ? containerGuid : null,
            Location = Path.Combine(d.FullName, name),
            SubscriptionId = subscriptionId,
            ModifiedOn = DateTime.UtcNow,
            Type = fileType,
            Extension = extension
        });

        await dbContext.SaveChangesAsync();

    }
    catch (Exception ex)
    {

    }
});

#endregion

#region Shares logic APIs
var shareAPIs = app.MapGroup("/api/shares");

shareAPIs.MapPost("/", async ([FromServices] ApplicationDbContext dbContext, [FromBody] ShareRequest model, HttpRequest request) =>
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

shareAPIs.MapDelete("/{id}", async ([FromServices] ApplicationDbContext dbContext, Guid id, HttpRequest request) =>
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

shareAPIs.MapPut("/{id}", async ([FromServices] ApplicationDbContext dbContext, Guid id, [FromBody] ShareRequest model, HttpRequest request) =>
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

shareAPIs.MapGet("/", async ([FromServices] ISharingService sharingService, [FromQuery] bool descending = false, [FromQuery] string? search = null, [FromQuery] string OrderBy = nameof(ShareRequest.Name), [FromQuery] int skip = 0, [FromQuery] int take = 25, CancellationToken cancellationToken = default) =>
{
    var result = await sharingService.QueryAsync(skip, take, OrderBy, descending, cancellationToken);
    return Results.Ok(result);
});

#endregion

app.Run();