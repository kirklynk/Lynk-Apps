using Azure.Core;
using DocumentManagementService;
using DocumentManagementService.Data;
using DocumentManagementService.Domain;
using DocumentManagementService.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using Shared.Models;
using Shared.Requests;
using System.IO;
using System.Runtime.CompilerServices;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContextPool<ApplicationDbContext>(o =>
{
    o.UseSqlServer(builder.Configuration.GetConnectionString("DmsDbConnection"));
});

builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(builder =>
        builder.Expire(TimeSpan.FromSeconds(10)));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseOutputCache();

var apis = app.MapGroup("/api/{subscriptionId}/documents");

/// Get root Container contents
apis.MapGet("/", async ([FromServices] ApplicationDbContext dbContext, Guid subscriptionId, HttpRequest request, [FromQuery] bool descending = false, [FromQuery] string? search = null, [FromQuery] string OrderBy = nameof(DocumentInfo.Name), [FromQuery] int skip = 0, [FromQuery] int take = 25, CancellationToken cancellationToken = default) =>
{
    try
    {
        if (skip < 0) skip = 0;
        if (take <= 0) take = 10;

        if (take > 100) take = 100;

        var userName = request.HttpContext?.User.Identity?.Name;
        var Containers = dbContext.Containers
            .Where(f => f.SubscriptionId == subscriptionId && f.ParentId == null)
            .Select(f => new { f.Id, f.Name, IsContainer = true, f.ModifiedOn, Type = "" });

        var documents = dbContext.Documents.Where(d => d.SubscriptionId == subscriptionId && d.ContainerId == null)
            .Select(d => new { d.Id, d.Name, IsContainer = false, d.ModifiedOn, d.Type });

        var query = Containers.Union(documents);

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(f => f.Name.Contains(search));
        }

        query = OrderBy.ToLower() switch
        {
            "name" => !descending ? query.OrderByDescending(x => x.IsContainer).ThenBy(f => f.Name) : query.OrderByDescending(x => x.IsContainer).ThenByDescending(f => f.Name),
            "modifiedon" => !descending ? query.OrderByDescending(x => x.IsContainer).ThenBy(f => f.ModifiedOn) : query.OrderByDescending(x => x.IsContainer).ThenByDescending(f => f.ModifiedOn),
            _ => query.OrderBy(f => f.Name)
        };

        var count = await query.CountAsync();
        query = query.Skip(skip).Take(take);
        var items = await query.Select(x => new DocumentInfo
        {
            Id = x.Id,
            Name = x.Name,
            IsContainer = x.IsContainer,
            ModifiedOn = x.ModifiedOn
        }).ToListAsync();

        return Results.Ok(new QuerySet<DocumentInfo>
        {
            Items = items,
            TotalCount = count
        });
    }
    catch (Exception)
    {
        return Results.Problem("An error occurred while creating the Container.");
    }
}).Produces<QuerySet<Shared.Models.DocumentInfo>>();

/// Get Container contents
apis.MapGet("/containers/{id}", async ([FromServices] ApplicationDbContext dbContext, Guid subscriptionId, Guid id, HttpRequest request, [FromQuery] bool descending = false, [FromQuery] string? search = null, [FromQuery] string OrderBy = nameof(DocumentInfo.Name), [FromQuery] int skip = 0, [FromQuery] int take = 10, CancellationToken cancellationToken = default) =>
{
    try
    {

        if (skip < 0) skip = 0;
        if (take <= 0) take = 10;

        var userName = request.HttpContext?.User.Identity?.Name;
        var Containers = dbContext.Containers
            .Where(f => f.SubscriptionId == subscriptionId && f.ParentId == id)
            .Select(f => new { f.Id, f.Name, IsContainer = true, f.ModifiedOn, Type = "" });

        var documents = dbContext.Documents.Where(d => d.SubscriptionId == subscriptionId && d.ContainerId == id)
            .Select(d => new { d.Id, d.Name, IsContainer = false, d.ModifiedOn, d.Type });

        var query = Containers.Union(documents);

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(f => f.Name.Contains(search));
        }

        query = OrderBy.ToLower() switch
        {
            "name" => !descending ? query.OrderByDescending(x => x.IsContainer).ThenBy(f => f.Name) : query.OrderByDescending(x => x.IsContainer).ThenByDescending(f => f.Name),
            "modifiedon" => !descending ? query.OrderByDescending(x => x.IsContainer).ThenBy(f => f.ModifiedOn) : query.OrderByDescending(x => x.IsContainer).ThenByDescending(f => f.ModifiedOn),
            _ => query.OrderByDescending(f => f.IsContainer)
        };

        var count = await query.CountAsync(cancellationToken: cancellationToken);

        query = query.Skip(skip).Take(take);

        var Container = await dbContext.Containers.Include(x => x.Parent).Where(x => x.Id == id && x.SubscriptionId == subscriptionId).FirstOrDefaultAsync(cancellationToken: cancellationToken);

        return Results.Ok(new QuerySet<DocumentInfo>
        {
            Name = Container?.Name,
            Parent = Container?.Parent != null ? new DocumentInfo { Id = Container.Parent.Id, Name = Container.Parent.Name } : null,
            Items = await query.Select(x => new DocumentInfo
            {
                Id = x.Id,
                Name = x.Name,
                IsContainer = x.IsContainer,
                ModifiedOn = x.ModifiedOn,
                Type = x.Type
            }).ToListAsync(cancellationToken: cancellationToken),
            TotalCount = count
        });
    }
    catch (Exception)
    {
        return Results.Problem("An error occurred while creating the Container.");
    }
}).Produces<QuerySet<DocumentInfo>>();

/// Get recycle bin contents
apis.MapGet("/recyclebin", async ([FromServices] ApplicationDbContext dbContext, Guid subscriptionId, HttpRequest request, [FromQuery] bool descending = false, [FromQuery] string? search = null, [FromQuery] string OrderBy = nameof(DocumentInfo.Name), [FromQuery] int skip = 0, [FromQuery] int take = 25, CancellationToken cancellationToken = default) =>
{
    try
    {
        if (skip < 0) skip = 0;
        if (take <= 0) take = 10;

        if (take > 100) take = 100;

        var userName = request.HttpContext?.User.Identity?.Name;
        var Containers = dbContext.Containers.IgnoreQueryFilters()
            .Where(f => f.SubscriptionId == subscriptionId && f.ParentId == null && f.IsDeleted)
            .Select(f => new { f.Id, f.Name, IsContainer = true, f.DeletedOn, f.ModifiedOn });

        var documents = dbContext.Documents
        .IgnoreQueryFilters()
        .Where(d => d.SubscriptionId == subscriptionId && d.ContainerId == null && d.IsDeleted)
            .Select(d => new { d.Id, d.Name, IsContainer = false, d.DeletedOn, d.ModifiedOn });

        var query = Containers.Union(documents);

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(f => f.Name.Contains(search));
        }

        query = OrderBy.ToLower() switch
        {
            "name" => !descending ? query.OrderByDescending(x => x.IsContainer).ThenBy(f => f.Name) : query.OrderByDescending(x => x.IsContainer).ThenByDescending(f => f.Name),
            "modifiedon" => !descending ? query.OrderByDescending(x => x.IsContainer).ThenBy(f => f.DeletedOn) : query.OrderByDescending(x => x.IsContainer).ThenByDescending(f => f.DeletedOn),
            _ => query.OrderBy(f => f.Name)
        };

        var count = await query.CountAsync();
        query = query.Skip(skip).Take(take);


        return Results.Ok(new QuerySet<DocumentInfo>
        {
            Items = await query.Select(x => new DocumentInfo()
            {
                Id = x.Id,
                Name = x.Name,
                IsContainer = x.IsContainer,
                DeletedOn = x.DeletedOn,
                ModifiedOn = x.ModifiedOn
            }).ToListAsync(),
            TotalCount = count
        });
    }
    catch (Exception)
    {
        return Results.Problem("An error occurred while creating the Container.");
    }
}).Produces<QuerySet<Shared.Models.DocumentInfo>>();

apis.MapPost("/recyclebin/empty", async ([FromServices] ApplicationDbContext dbContext, Guid subscriptionId, HttpRequest request) =>
{

    await dbContext.Containers.Where(x => x.SubscriptionId == subscriptionId && x.IsDeleted).ExecuteDeleteAsync();
    await dbContext.Documents.Where(x => x.SubscriptionId == subscriptionId && x.IsDeleted).ExecuteDeleteAsync();
    return Results.Ok();
});

apis.MapPost("/recyclebin/restore/{id}", async ([FromServices] ApplicationDbContext dbContext, [FromRoute] Guid subscriptionId, [FromRoute] Guid id, HttpRequest request) =>
{

    await dbContext.Containers.IgnoreQueryFilters().Where(x => x.SubscriptionId == subscriptionId && x.IsDeleted && x.Id == id).ExecuteUpdateAsync(x =>
        x.SetProperty(c => c.IsDeleted, false)
         .SetProperty(c => c.DeletedOn, (DateTime?)null)
    );

    await dbContext.Documents.IgnoreQueryFilters().Where(x => x.SubscriptionId == subscriptionId && x.IsDeleted && x.Id == id).ExecuteUpdateAsync(x =>
    {
        x.SetProperty(c => c.IsDeleted, false);
        x.SetProperty(c => c.DeletedOn, (DateTime?)null);
    });

    return Results.Ok();
});

/// Get Container details
apis.MapGet("/containers/{id}/details", async ([FromServices] ApplicationDbContext dbContext, Guid subscriptionId, Guid id) =>
{
    var Container = await dbContext.Containers.Include(x => x.Parent)
                .Where(x => x.Id == id && x.SubscriptionId == subscriptionId)
                .FirstOrDefaultAsync();

    if (Container == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(new DocumentInfo
    {
        Id = Container.Id,
        Name = Container.Name,
        Parent = Container.Parent != null ? new DocumentInfo { Id = Container.Parent.Id, Name = Container.Parent.Name } : null
    });

}).Produces<DocumentInfo>();


apis.MapDelete("/{id}", async ([FromServices] ApplicationDbContext dbContext, Guid subscriptionId, Guid id, bool isPermanent = false, CancellationToken cancellationToken = default) =>
{
    if (id == Guid.Empty)
    {
        return Results.ValidationProblem(new Dictionary<string, string[]> { { "id", new[] { "Container id is required." } } });
    }

    if (subscriptionId == Guid.Empty)
    {
        return Results.ValidationProblem(new Dictionary<string, string[]> { { "subscriptionId", new[] { "Subscription id is required." } } });
    }

    var Container = await dbContext.Containers.Include(x => x.Documents).FirstOrDefaultAsync(x => x.Id == id && x.SubscriptionId == subscriptionId, cancellationToken);
    if (Container != null)
    {
        if (isPermanent)
        {
            dbContext.Containers.Remove(Container);
        }
        else
        {
            Container.IsDeleted = true;
            Container.DeletedOn = DateTime.UtcNow;
        }
    }
    else
    {
        var document = await dbContext.Documents.FirstOrDefaultAsync(x => x.Id == id && x.SubscriptionId == subscriptionId, cancellationToken);
        if (document != null)
        {
            if (isPermanent)
            {
                dbContext.Documents.Remove(document);
            }
            else
            {
                document.IsDeleted = true;
                document.DeletedOn = DateTime.UtcNow;
            }
        }
    }
    await dbContext.SaveChangesAsync(cancellationToken);
    return Results.Ok();
});

/// Create a new Container
apis.MapPost("/containers", async ([FromServices] ApplicationDbContext dbContext, Guid subscriptionId, CreateContainer request) =>
{
    if (string.IsNullOrWhiteSpace(request.Name))
    {
        return Results.BadRequest("Container name is required.");
    }

    try
    {
        var found = await dbContext.Containers.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Name == request.Name.Trim() && x.SubscriptionId == subscriptionId);
        if (found != null)
        {
            if (found.IsDeleted)
            {
                return Results.Problem("Cannot create a new {{Container}}+ with the same name as a deleted one.", statusCode: StatusCodes.Status410Gone);
            }
            return Results.Problem("A Container with the same name already exists.", statusCode: StatusCodes.Status409Conflict);
        }

        var Container = new LynkContainer
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            ParentId = request.ParentId,
            SubscriptionId = subscriptionId,
            ModifiedOn = DateTime.UtcNow
        };

        dbContext.Containers.Add(Container);
        await dbContext.SaveChangesAsync();

        return Results.Ok(new DocumentInfo
        {
            Id = Container.Id,
            Name = Container.Name,
            Parent = request.ParentId.HasValue ? new DocumentInfo { Id = request.ParentId.Value } : null
        });
    }
    catch (Exception)
    {
        return Results.Problem("An error occurred while creating the Container.");
    }
}).Produces<DocumentInfo>()
    .ProducesProblem(StatusCodes.Status500InternalServerError);

apis.MapPost("/upload/stream", async ([FromServices] ApplicationDbContext dbContext, HttpRequest request, Guid subscriptionId, [FromHeader(Name = "X-ContainerId")] string? containerId) =>
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

        var d = Directory.CreateDirectory("uploads");
        var name = $"{Guid.NewGuid()}{extension}";
        var path = Path.Combine("uploads", name);

        await using var fs = new FileStream(
            path,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 1024 * 1024,
            useAsync: true);

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

        await request.Body.CopyToAsync(fs);
        await dbContext.SaveChangesAsync();

    }
    catch (Exception ex)
    {

    }
});

app.Run();