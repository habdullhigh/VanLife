using System.Threading.Tasks;
using VanLife.Api.Models;

namespace VanLife.Api.Data.Repositories;

public interface IVanRepository : IRepository<Van>
{
    Task<Van?> GetByIdWithPhotosAsync(Guid id);
}
