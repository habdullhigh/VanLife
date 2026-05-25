using System.Linq;
using System.Threading.Tasks;

namespace VanLife.Api.Data.Repositories;

public interface IRepository<T> where T : class
{
    IQueryable<T> Query();
    Task<T?> GetByIdAsync(Guid id);
    Task AddAsync(T entity);
    void Update(T entity);
    void Remove(T entity);
    Task<int> SaveChangesAsync();
}
