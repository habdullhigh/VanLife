using VanLife.Api.Data;

namespace VanLife.Api.Services;

public class IncomeService(InMemoryStore store)
{
    public object GetYearlyGraph(int year)
    {
        var transactions = store.Transactions.Where(t => t.Date.Year == year).ToList();

        var monthly = Enumerable.Range(1, 12)
            .Select(month =>
            {
                var monthTransactions = transactions.Where(t => t.Date.Month == month).ToList();
                return new
                {
                    month,
                    transactionCount = monthTransactions.Count,
                    totalValue = monthTransactions.Sum(t => t.Price)
                };
            });

        return new { year, monthly };
    }

    public object GetTransactions(int? days)
    {
        var query = store.Transactions.AsEnumerable();
        if (days.HasValue)
        {
            var from = DateTime.UtcNow.AddDays(-days.Value);
            query = query.Where(t => t.Date >= from);
        }

        var list = query
            .OrderByDescending(t => t.Date)
            .Select(t => new { t.Id, t.SellerId, t.VanId, t.Price, t.Date });

        return new
        {
            total = list.Count(),
            items = list
        };
    }
}

