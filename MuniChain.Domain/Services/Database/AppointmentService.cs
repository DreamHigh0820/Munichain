using Data.DatabaseServices;
using Microsoft.EntityFrameworkCore;
using Shared.Models.DealComponents;

namespace Domain.Services.Database
{
    public interface IAppointmentService
    {
        Task<bool> Create(AppointmentData appt);
        Task<bool> Delete(string id);
        Task<List<AppointmentData>> GetByDealId(string DealId);
        Task<AppointmentData> GetById(string id);
        Task<bool> Update(AppointmentData appt);
    }

    public class AppointmentService : IAppointmentService
    {
        private readonly IDbContextFactory<SqlDbContext> _factory;

        public AppointmentService(IDbContextFactory<SqlDbContext> factory)
        {
            _factory = factory;
        }


        public async Task<bool> Create(AppointmentData appt)
        {
            using (var _dbContext = _factory.CreateDbContext())
            {
                _dbContext.Add(appt);
                try
                {
                    await _dbContext.SaveChangesAsync();
                    return true;
                }
                catch(Exception)
                {
                    Console.WriteLine("ERROR");
                    return false;
                }
            }
            
        }
        public async Task<bool> Delete(string id)
        {
            using (var _dbContext = _factory.CreateDbContext())
            {
                var appt = await _dbContext.AppointmentData.FindAsync(id);
                if (appt == null)
                {
                    return false;
                }

                _dbContext.AppointmentData.Remove(appt);
                await _dbContext.SaveChangesAsync();
                return true;
            }
        }
        public async Task<List<AppointmentData>> GetByDealId(string DealId)
        {
            using (var _dbContext = _factory.CreateDbContext())
            {
                return await _dbContext.AppointmentData.Where(x => x.DealModelId == DealId).ToListAsync();
            }
        }

        public async Task<AppointmentData> GetById(string id)
        {
            using (var _dbContext = _factory.CreateDbContext())
            {
                return await _dbContext.AppointmentData.FirstOrDefaultAsync(x => x.Id == id);
            }
        }

        public async Task<bool> Update(AppointmentData appt)
        {
            using (var _dbContext = _factory.CreateDbContext())
            {
                var foundAppt = await GetById(appt.Id);
                if(foundAppt == null)
                {
                    return await Create(appt);
                }
                _dbContext.Entry(appt).State = EntityState.Modified;
                await _dbContext.SaveChangesAsync();
                    return true;
            }
        }
       
    }
}
