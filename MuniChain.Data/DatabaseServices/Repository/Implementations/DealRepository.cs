using Data.DatabaseServices.Repository;
using Microsoft.EntityFrameworkCore;
using Shared.Models.DealComponents;

namespace Data.DatabaseServices.Repository.Implementations
{
    public interface IDealRepository : IRepository<DealModel>
    {
    }
    public class DealRepository : Repository<DealModel>, IDealRepository
    {
        public DealRepository(IDbContextFactory<SqlDbContext> context) : base(context)
        {
        }
    }
}
