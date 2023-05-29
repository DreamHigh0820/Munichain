using Data.DatabaseServices;
using Microsoft.EntityFrameworkCore;
using Shared.Models.DealComponents;

namespace Domain.Services.Database
{
    public interface IReferenceService
    {
        Task<bool> Create(Reference reference);
        Task<bool> Delete(string id);
        Task<List<Reference>> GetByDealId(string dealId);
        Task<List<Reference>> GetByUserEmail(string email);
        Task<List<Reference>> GetForFirm(string id);
        Task<List<Reference>> Search();
        Task<bool> Update(Reference reference);
    }

    public class ReferenceService : IReferenceService
    {
        private readonly IDbContextFactory<SqlDbContext> _factory;

        public ReferenceService(IDbContextFactory<SqlDbContext> factory)
        {
            _factory = factory;
        }

        public async Task<List<Reference>> Search()
        {
            using (var _dbContext = _factory.CreateDbContext())
            {
                return await _dbContext.Reference.ToListAsync();
            }
        }
        public async Task<List<Reference>> GetByDealId(string dealId)
        {
            using (var _dbContext = _factory.CreateDbContext())
            {
                return await _dbContext.Reference.Where(x => x.DealId == dealId).ToListAsync();
            }
        }

        public async Task<List<Reference>> GetByUserEmail(string email)
        {
            using (var _dbContext = _factory.CreateDbContext())
            {
                return await _dbContext.Reference.Where(x => x.GivenBy == email).ToListAsync();
            }
        }
        public async Task<List<Reference>> GetForFirm(string id)
        {
            using (var _dbContext = _factory.CreateDbContext())
            {
                return await _dbContext.Reference.Where(x => x.FirmId == id).ToListAsync();
            }
        }

        public async Task<bool> Create(Reference reference)
        {
            using (var _dbContext = _factory.CreateDbContext())
            {
                reference.Id = Guid.NewGuid().ToString();
                _dbContext.Add(reference);
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

        public async Task<bool> Update(Reference reference)
        {
            using (var _dbContext = _factory.CreateDbContext())
            {
                _dbContext.Entry(reference).State = EntityState.Modified;
                await _dbContext.SaveChangesAsync();
                return true;
            }
        }

        public async Task<bool> Delete(string id)
        {
            using (var _dbContext = _factory.CreateDbContext())
            {
                var reference = await _dbContext.Reference.FindAsync(id);
                if (reference == null)
                {
                    return false;
                }

                _dbContext.Reference.Remove(reference);
                await _dbContext.SaveChangesAsync();
                return true;
            }
        }
    }
}
