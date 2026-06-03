using Microsoft.EntityFrameworkCore;
using VanLife.Api.Data;
using VanLife.Api.Models;

namespace VanLife.Api.Data.Repositories;

public class VanRepository : EfRepository<Van>, IVanRepository
{
    private readonly AppDbContext _db;

    public VanRepository(AppDbContext db) : base(db)
    {
        _db = db;
    }

    public async Task<Van?> GetByIdWithPhotosAsync(Guid id)
    {
        return await _db.Vans.Include(v => v.Photos).FirstOrDefaultAsync(v => v.Id == id);
    }
}
