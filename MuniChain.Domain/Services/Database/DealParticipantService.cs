using Microsoft.EntityFrameworkCore;
using Shared.Models.Users;
using Shared.Models.DealComponents;
using Data.DatabaseServices;

namespace Domain.Services.Database
{
    public interface IDealParticipantService
    {
        public Task<List<DealParticipant>> GetParticipantsByDealId(string id);
        public Task<List<string>> GetDealsByUser(string userId);
        public Task<bool> CreateParticipant(DealParticipant dealParticipant);
        public Task<bool> UpdateParticipants(List<DealParticipant> dealParticipants);
        public Task<bool> UpdateParticipantPermissions(DealParticipant dealParticipant);
        Task<bool> ParticipantExistsOnDeal(string email, string dealId);
        public Task<bool> UpdateParticipantForNewUser(User user);
        public Task<bool> DeleteParticipant(DealParticipant dealParticipant);
    }
    public class DealParticipantService : IDealParticipantService
    {
        private readonly IDbContextFactory<SqlDbContext> _factory;

        public DealParticipantService(IDbContextFactory<SqlDbContext> factory)
        {
            _factory = factory;
        }

        public async Task<List<DealParticipant>> GetParticipantsByDealId(string id)
        {
            using (var _dbContext = _factory.CreateDbContext())
            {
                return await _dbContext.DealParticipants.Where(x => x.DealId == id).ToListAsync();
            }
        }

        public async Task<bool> ParticipantExistsOnDeal(string email, string dealId)
        {
            using (var _dbContext = _factory.CreateDbContext())
            {
                return await _dbContext.DealParticipants.AnyAsync(x => x.EmailAddress == email && x.DealId == dealId);
            }
        }

        public async Task<List<string>> GetDealsByUser(string userId)
        {
            using (var _dbContext = _factory.CreateDbContext())
            {
                return await _dbContext.DealParticipants.Where(x => x.UserId == userId).Select(x => x.DealId).ToListAsync();
            }
        }

        private async Task<List<DealParticipant>> GetDealsByEmail(string email)
        {
            using (var _dbContext = _factory.CreateDbContext())
            {
                return await _dbContext.DealParticipants.Where(x => x.EmailAddress == email).ToListAsync();
            }
        }

        public async Task<bool> CreateParticipant(DealParticipant dealParticipant)
        {
            using (var _dbContext = _factory.CreateDbContext())
            {
                // Delete current participants
                _dbContext.Add(dealParticipant);
                try
                {
                    await _dbContext.SaveChangesAsync();
                    return true;
                }
                catch (Exception)
                {
                    Console.WriteLine("ERROR");
                    return false;
                }
            }
        }

        public async Task<bool> UpdateParticipantPermissions(DealParticipant dealParticipant)
        {
            using (var _dbContext = _factory.CreateDbContext())
            {
                var dealFromDb = await _dbContext.DealParticipants.AsNoTracking().FirstOrDefaultAsync(x => x.Id == dealParticipant.Id);
                if (dealFromDb != null)
                {
                    if (!dealFromDb.RowVersion.SequenceEqual(dealParticipant.RowVersion))
                    {
                        throw new DbUpdateConcurrencyException();
                    }
                }
                _dbContext.DealParticipants.Attach(dealParticipant);
                _dbContext.Entry(dealParticipant).Property(x => x.DealPermissions).IsModified = true;
                await _dbContext.SaveChangesAsync();
                return true;
            }
        }

        public async Task<bool> UpdateParticipants(List<DealParticipant> dealParticipants)
        {
            using (var _dbContext = _factory.CreateDbContext())
            {
                foreach (var dealParticipant in dealParticipants)
                {
                    var dealFromDb = await _dbContext.DealParticipants.AsNoTracking().FirstOrDefaultAsync(x => x.Id == dealParticipant.Id);
                    if (dealFromDb != null)
                    {
                        if (!dealFromDb.RowVersion.SequenceEqual(dealParticipant.RowVersion))
                        {
                            throw new DbUpdateConcurrencyException();
                        }
                    }
                }
                _dbContext.UpdateRange(dealParticipants);
                await _dbContext.SaveChangesAsync();
                return true;
            }
        }
        public async Task<bool> UpdateParticipantForNewUser(User user)
        {
            using (var _dbContext = _factory.CreateDbContext())
            {
                var dealParticipantList = await GetDealsByEmail(user.Email);

                if (!dealParticipantList.Any())
                {
                    return false;
                }

                foreach (var dealParticipant in dealParticipantList)
                {
                    dealParticipant.DisplayName = user.DisplayName;
                    dealParticipant.UserId = user.Id;
                }

                _dbContext.DealParticipants.UpdateRange(dealParticipantList);
                await _dbContext.SaveChangesAsync();
                return true;
            }
        }

        public async Task<bool> DeleteParticipant(DealParticipant dealParticipant)
        {
            using (var _dbContext = _factory.CreateDbContext())
            {
                _dbContext.DealParticipants.Remove(dealParticipant);
                await _dbContext.SaveChangesAsync();
                return true;
            }
        }
    }
}
