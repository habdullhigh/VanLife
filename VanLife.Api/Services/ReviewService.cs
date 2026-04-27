using VanLife.Api.Data;
using VanLife.Api.Models;

namespace VanLife.Api.Services;

public class ReviewService(InMemoryStore store)
{
    public object GetUserReviews(Guid userId, DateTime? startDate, DateTime? endDate, ReviewType? reviewType)
    {
        var query = store.Reviews.Where(r => r.TargetUserId == userId);

        if (startDate.HasValue)
        {
            query = query.Where(r => r.Date >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(r => r.Date <= endDate.Value);
        }

        var filtered = query.ToList();
        var total = filtered.Count;

        if (reviewType.HasValue)
        {
            filtered = [.. filtered.Where(r => r.Type == reviewType.Value)];
        }

        var specificCount = filtered.Count;
        var percentage = total == 0 ? 0 : (double)specificCount / total * 100;

        return new
        {
            totalReviews = total,
            specificReviews = specificCount,
            reviewPercentage = Math.Round(percentage, 2),
            items = filtered.Select(r => new { r.Id, r.Date, r.Type, r.Stars, r.Comment })
        };
    }
}

