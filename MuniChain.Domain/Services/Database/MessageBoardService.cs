using Data.DatabaseServices;
using Microsoft.EntityFrameworkCore;
using Shared.Models.DealComponents;

namespace Domain.Services.Database
{
    public interface IMessageBoardService
    {
        Task<bool> Create(BoardMessage msg);
        Task<List<BoardMessage>> SearchByDealId(string id);
    }

    public class MessageBoardService : IMessageBoardService
    {
        public MessageBoardService(IDbContextFactory<SqlDbContext> factory)
        {
            _factory = factory;
        }

        private readonly IDbContextFactory<SqlDbContext> _factory;

        public async Task<List<BoardMessage>> SearchByDealId(string id)
        {
            using (var _dbContext = _factory.CreateDbContext())
            {
                return await _dbContext.BoardMessages.Where(x => x.DealId == id).OrderByDescending(x => x.DateGivenUTC).ToListAsync();
            }
        }

        public async Task<bool> Create(BoardMessage msg)
        {
            using (var _dbContext = _factory.CreateDbContext())
            {
                _dbContext.Add(msg);
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
    }
}
