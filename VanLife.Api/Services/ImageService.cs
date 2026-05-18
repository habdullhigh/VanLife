using Microsoft.EntityFrameworkCore;
using VanLife.Api.Data;
using VanLife.Api.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace VanLife.Api.Services;

public class ImageService
{
    private readonly AppDbContext db;
    private readonly IWebHostEnvironment env;

    public ImageService(AppDbContext db, IWebHostEnvironment env)
    {
        this.db = db;
        this.env = env;
    }

    public async Task<ImageAsset> Upload(IFormFile file, Guid? vanId = null)
    {
        var uploads = Path.Combine(env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "images");
        Directory.CreateDirectory(uploads);
        var fileName = $"{Guid.NewGuid():N}-{Path.GetFileName(file.FileName)}";
        var filePath = Path.Combine(uploads, fileName);
        await using var stream = File.Create(filePath);
        await file.CopyToAsync(stream);

        var url = $"/images/{fileName}"; // served by static files

        var image = new ImageAsset
        {
            Id = Guid.NewGuid(),
            VanId = vanId,
            FileName = file.FileName,
            Url = url,
            UploadedAt = DateTime.UtcNow
        };

        db.Images.Add(image);
        await db.SaveChangesAsync();
        return image;
    }

    public async Task<PagedResult<object>> GetAll(PaginationQuery query)
    {
        var images = db.Images.AsQueryable();
        var total = await images.CountAsync();
        var safePage = Math.Max(1, query.Page);
        var safePageSize = Math.Clamp(query.PageSize, 1, 100);
        var pageSkip = (safePage - 1) * safePageSize;
        var skip = Math.Max(query.Skip, pageSkip);

        var list = await images
            .OrderByDescending(i => i.UploadedAt)
            .Skip(skip)
            .Take(safePageSize)
            .Select(i => new { i.Id, i.VanId, i.FileName, i.Url, i.UploadedAt })
            .ToListAsync();

        return new PagedResult<object>(list.Cast<object>(), total, safePage, safePageSize, skip);
    }

    public async Task<bool> Delete(Guid id)
    {
        var image = await db.Images.FirstOrDefaultAsync(i => i.Id == id);
        if (image is null)
        {
            return false;
        }

        // attempt to delete file on disk if stored locally
        try
        {
            if (!string.IsNullOrWhiteSpace(image.Url) && image.Url.StartsWith("/images/"))
            {
                var filePath = Path.Combine(env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), image.Url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (File.Exists(filePath)) File.Delete(filePath);
            }
        }
        catch { /* best-effort */ }

        db.Images.Remove(image);
        await db.SaveChangesAsync();
        return true;
    }
}

