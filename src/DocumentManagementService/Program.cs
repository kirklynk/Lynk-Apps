using DocumentManagementService;
using DocumentManagementService.Data;
using DocumentManagementService.Domain;
using DocumentManagementService.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Models;
using Shared.Requests;
using System.Runtime.CompilerServices;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContextPool<ApplicationDbContext>(o =>
{
    o.UseSqlServer(builder.Configuration.GetConnectionString("DmsDbConnection"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var apis = app.MapGroup("/api/{subscriptionId}/files");

/// Get root folder contents
apis.MapGet("/", async ([FromServices] ApplicationDbContext dbContext, Guid subscriptionId, HttpRequest request, [FromQuery] bool descending = false, [FromQuery] string? search = null, [FromQuery] string OrderBy = nameof(LynkFileInfo.Name), [FromQuery] int page = 0, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default) =>
{
    try
    {
        if (page < 0) page = 0;
        if (pageSize <= 0) pageSize = 10;

        var userName = request.HttpContext?.User.Identity?.Name;
        var folders = dbContext.Folders
            .Where(f => f.SubscriptionId == subscriptionId && f.ParentId == null)
            .Select(f => new { f.Id, f.Name, IsFolder = true, f.ModifiedOn });

        var documents = dbContext.Documents.Where(d => d.SubscriptionId == subscriptionId && d.FolderId == null)
            .Select(d => new { d.Id, d.Name, IsFolder = true, d.ModifiedOn });

        var query = folders.Union(documents);

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(f => f.Name.Contains(search));
        }

        query = OrderBy.ToLower() switch
        {
            "name" => !descending ? query.OrderByDescending(x => x.IsFolder).ThenBy(f => f.Name) : query.OrderByDescending(x => x.IsFolder).ThenByDescending(f => f.Name),
            "modifiedon" => !descending ? query.OrderByDescending(x => x.IsFolder).ThenBy(f => f.ModifiedOn) : query.OrderByDescending(x => x.IsFolder).ThenByDescending(f => f.ModifiedOn),
            _ => query.OrderBy(f => f.Name)
        };

        var count = await query.CountAsync();
        query = query.Skip(page).Take(pageSize);


        return Results.Ok(new QuerySet<LynkFileInfo>
        {
            Items = await query.Select(x => new LynkFileInfo()
            {
                Id = x.Id,
                Name = x.Name,
                IsFolder = x.IsFolder,
                ModifiedOn = x.ModifiedOn,
            }).ToListAsync(),
            TotalCount = count
        });
    }
    catch (Exception)
    {
        return Results.Problem("An error occurred while creating the folder.");
    }
}).Produces<QuerySet<Shared.Models.LynkFileInfo>>();

/// Get folder details
apis.MapGet("/folders/{id}/details", async ([FromServices] ApplicationDbContext dbContext, Guid subscriptionId, Guid id) =>
{
    var folder = await dbContext.Folders.Include(x => x.Parent)
                .Where(x => x.Id == id && x.SubscriptionId == subscriptionId)
                .FirstOrDefaultAsync();

    if (folder == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(new FileDetails
    {
        Id = folder.Id,
        Name = folder.Name,
        Parent = folder.Parent != null ? new FileDetails { Id = folder.Parent.Id, Name = folder.Parent.Name } : null
    });

}).Produces<FileDetails>();

/// Get folder contents
apis.MapGet("/folders/{id}", async ([FromServices] ApplicationDbContext dbContext, Guid subscriptionId, Guid id, HttpRequest request, [FromQuery] bool descending = false, [FromQuery] string? search = null, [FromQuery] string OrderBy = nameof(LynkFileInfo.Name), [FromQuery] int page = 0, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default) =>
{
    try
    {

        if (page <= 0) page = 0;
        if (pageSize <= 0) pageSize = 10;

        var userName = request.HttpContext?.User.Identity?.Name;
        var folders = dbContext.Folders
            .Where(f => f.SubscriptionId == subscriptionId && f.ParentId == id)
            .Select(f => new { f.Id, f.Name, IsFolder = true, f.ModifiedOn });

        var documents = dbContext.Documents.Where(d => d.SubscriptionId == subscriptionId && d.FolderId == id)
            .Select(d => new { d.Id, d.Name, IsFolder = false, d.ModifiedOn });

        var query = folders.Union(documents);

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(f => f.Name.Contains(search));
        }

        query = OrderBy.ToLower() switch
        {
            "name" => !descending ? query.OrderByDescending(x => x.IsFolder).ThenBy(f => f.Name) : query.OrderByDescending(x => x.IsFolder).ThenByDescending(f => f.Name),
            "modifiedon" => !descending ? query.OrderByDescending(x => x.IsFolder).ThenBy(f => f.ModifiedOn) : query.OrderByDescending(x => x.IsFolder).ThenByDescending(f => f.ModifiedOn),
            _ => query.OrderByDescending(f => f.IsFolder)
        };

        var count = await query.CountAsync(cancellationToken: cancellationToken);

        query = query.Skip(page).Take(pageSize);

        var folder = await dbContext.Folders.Include(x => x.Parent).Where(x => x.Id == id && x.SubscriptionId == subscriptionId).FirstOrDefaultAsync(cancellationToken: cancellationToken);

        return Results.Ok(new QuerySet<LynkFileInfo>
        {
            Name = folder?.Name,
            Parent = folder?.Parent != null ? new LynkFileInfo { Id = folder.Parent.Id, Name = folder.Parent.Name } : null,
            Items = await query.Select(x => new LynkFileInfo
            {
                Id = x.Id,
                Name = x.Name,
                IsFolder = x.IsFolder,
                ModifiedOn = x.ModifiedOn
            }).ToListAsync(cancellationToken: cancellationToken),
            TotalCount = count
        });
    }
    catch (Exception)
    {
        return Results.Problem("An error occurred while creating the folder.");
    }
}).Produces<QuerySet<LynkFileInfo>>();

/// Create a new folder
apis.MapPost("/folders", async ([FromServices] ApplicationDbContext dbContext, Guid subscriptionId, CreateFolder request) =>
{
    if (string.IsNullOrWhiteSpace(request.Name))
    {
        return Results.BadRequest("Folder name is required.");
    }

    try
    {
        var folder = new LynkFolder
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            ParentId = request.ParentId,
            SubscriptionId = subscriptionId,
            ModifiedOn = DateTime.UtcNow
        };

        dbContext.Folders.Add(folder);
        await dbContext.SaveChangesAsync();

        return Results.Ok(new FileDetails
        {
            Id = folder.Id,
            Name = folder.Name,
            Parent = request.ParentId.HasValue ? new FileDetails { Id = request.ParentId.Value } : null
        });
    }
    catch (Exception)
    {
        return Results.Problem("An error occurred while creating the folder.");
    }
}).Produces<FileDetails>()
    .ProducesProblem(StatusCodes.Status500InternalServerError);

app.Run();


