using Microsoft.EntityFrameworkCore;
using VanLife.Api.Data;
using VanLife.Api.Models;

namespace VanLife.Api.Services;

public class ReviewService(AppDbContext db)
{
    private static DateTime NormalizeToUtc(DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
    }

    public async Task<object> GetUserReviews(ReviewQuery query)
    {
        var reviews = db.Reviews.Where(r => r.TargetUserId == query.UserId);

        if (query.StartDate.HasValue)
        {
            var startUtc = NormalizeToUtc(query.StartDate.Value);
            reviews = reviews.Where(r => r.Date >= startUtc);
        }

        if (query.EndDate.HasValue)
        {
            var endUtc = NormalizeToUtc(query.EndDate.Value);
            reviews = reviews.Where(r => r.Date <= endUtc);
        }

        var filtered = await reviews.ToListAsync();
        var total = filtered.Count;

        if (query.ReviewType.HasValue)
        {
            filtered = filtered.Where(r => r.Type == query.ReviewType.Value).ToList();
        }

        var specificCount = filtered.Count;
        var percentage = total == 0 ? 0 : (double)specificCount / total * 100;
        var safePage = Math.Max(1, query.Page);
        var safePageSize = Math.Clamp(query.PageSize, 1, 100);
        var pageSkip = (safePage - 1) * safePageSize;
        var skip = Math.Max(query.Skip, pageSkip);

        var pageItems = filtered
            .OrderByDescending(r => r.Date)
            .Skip(skip)
            .Take(safePageSize)
            .ToList();

        return new
        {
            totalReviews = total,
            specificReviews = specificCount,
            reviewPercentage = Math.Round(percentage, 2),
            page = safePage,
            pageSize = safePageSize,
            skip,
            items = pageItems.Select(r => new { r.Id, r.Date, r.Type, r.Stars, r.Comment })
        };
    }

    public async Task<OperationResult> CreateReview(CreateReviewRequest request)
    {
        // minimal validation: ensure target user exists
        var target = await db.Users.AnyAsync(u => u.Id == request.TargetUserId);
        if (!target) return new OperationResult(false, "Target user not found.");

        var review = new Review
        {
            Id = Guid.NewGuid(),
            TargetUserId = request.TargetUserId,
            Type = request.Type,
            Stars = request.Stars,
            Comment = request.Comment ?? string.Empty,
            Date = DateTime.UtcNow
        };

        db.Reviews.Add(review);
        await db.SaveChangesAsync();
        return new OperationResult(true, "Review created.");
    }
}

