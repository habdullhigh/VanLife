using Microsoft.EntityFrameworkCore;
using VanLife.Api.Data;

namespace VanLife.Api.Data.Repositories;

public class EfRepository<T> : IRepository<T> where T : class
{
    private readonly AppDbContext _db;
    private readonly DbSet<T> _set;

    public EfRepository(AppDbContext db)
    {
        _db = db;
        _set = db.Set<T>();
    }

    public IQueryable<T> Query() => _set.AsQueryable();

    public async Task<T?> GetByIdAsync(Guid id)
    {
        return await _set.FindAsync(id);
    }

    public async Task AddAsync(T entity)
    {
        await _set.AddAsync(entity);
    }

    public void Update(T entity)
    {
        _set.Update(entity);
    }

    public void Remove(T entity)
    {
        _set.Remove(entity);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _db.SaveChangesAsync();
    }
}
