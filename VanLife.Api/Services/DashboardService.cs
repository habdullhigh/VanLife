using VanLife.Api.Data;

namespace VanLife.Api.Services;

public class DashboardService(InMemoryStore store)
{
    public object GetSellerIncome(Guid sellerId, int? days)
    {
        var query = store.Transactions.Where(t => t.SellerId == sellerId);
        if (days.HasValue)
        {
            var from = DateTime.UtcNow.AddDays(-days.Value);
            query = query.Where(t => t.Date >= from);
        }

        var list = query.OrderByDescending(t => t.Date).ToList();

        return new
        {
            totalIncome = list.Sum(x => x.Price),
            totalTransactions = list.Count,
            transactions = list.Select(x => new { x.Id, x.VanId, x.Price, x.Date })
        };
    }
}

