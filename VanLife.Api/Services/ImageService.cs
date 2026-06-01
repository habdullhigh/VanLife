using Microsoft.EntityFrameworkCore;
using VanLife.Api.Data;
using VanLife.Api.Models;

namespace VanLife.Api.Services;

public class ImageService(AppDbContext db)
{
    public async Task<ImageAsset> Upload(string fileName, Guid? vanId = null)
    {
        var image = new ImageAsset
        {
            Id = Guid.NewGuid(),
            VanId = vanId,
            FileName = fileName,
            Url = $"https://cdn.vanlife.fake/images/{Guid.NewGuid():N}-{fileName}",
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

        db.Images.Remove(image);
        await db.SaveChangesAsync();
        return true;
    }
}

