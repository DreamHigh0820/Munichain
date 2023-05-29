using Data.DatabaseServices.Repository.Implementations;
using Microsoft.Extensions.Logging;
using Shared.Models.Users;

namespace Domain.Services.Database
{
    public interface INotificationPreferenceService
    {
        Task<bool> Create(UserNotificationPreference preference);
        Task<UserNotificationPreference> Get(string userId);
        Task<bool> Update(UserNotificationPreference preference);
    }

    public class NotificationPreferenceService : INotificationPreferenceService
    {
        public NotificationPreferenceService(INotificationPreferenceRepository preferences, ILogger<DocumentService> logger)
        {
            _preferences = preferences;
            _logger = logger;
        }

        private readonly INotificationPreferenceRepository _preferences;
        private readonly ILogger<DocumentService> _logger;

        public async Task<bool> Create(UserNotificationPreference preference)
        {
            await _preferences.Add(preference);
            return true;
        }

        public async Task<bool> Update(UserNotificationPreference preference)
        {
            try
            {
                await _preferences.Update(preference);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to update preference", ex);
                return false;
            }
        }

        public async Task<UserNotificationPreference> Get(string? userId)
        {
            var found = (await _preferences.Find(x => x.UserID == userId))?.FirstOrDefault();
            if (found is null)
            {
                UserNotificationPreference preference = new UserNotificationPreference();
                preference.UserID = userId;
                await _preferences.Add(preference);
                return preference;
            }
            else
                return found;
        }
    }
}
