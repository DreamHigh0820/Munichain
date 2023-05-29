using Data.DatabaseServices.Repository;
using Microsoft.EntityFrameworkCore;
using Shared.Models.DealComponents;

namespace Data.DatabaseServices.Repository.Implementations
{
    public interface IAppointmentRepository : IRepository<AppointmentData>
    {
    }
    public class AppointmentRepository : Repository<AppointmentData>, IAppointmentRepository
    {
        public AppointmentRepository(IDbContextFactory<SqlDbContext> context) : base(context)
        {
        }
    }
}
