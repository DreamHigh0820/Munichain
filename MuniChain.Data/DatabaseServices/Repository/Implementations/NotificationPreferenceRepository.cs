using Data.DatabaseServices;
using Data.DatabaseServices.Repository;
using Microsoft.EntityFrameworkCore;
using Shared.Models.DealComponents;
using Shared.Models.Users;

namespace Data.DatabaseServices.Repository.Implementations
{
    public interface INotificationPreferenceRepository : IRepository<UserNotificationPreference>
    {
    }
    public class NotificationPreferenceRepository : Repository<UserNotificationPreference>, INotificationPreferenceRepository
    {
        public NotificationPreferenceRepository(IDbContextFactory<SqlDbContext> context) : base(context)
        {
        }
    }
}
