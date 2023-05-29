using Data.DatabaseServices;
using Microsoft.EntityFrameworkCore;
using Shared.Models.Chat;

namespace Domain.Services.Database
{
    public interface IChatService
    {
        Task Create(ChatMessage message);
        Task<List<ChatMessage>> GetMessages(int conversationId);
    }

    public class ChatService : IChatService
    {
        private readonly IDbContextFactory<SqlDbContext> _factory;

        public ChatService(IDbContextFactory<SqlDbContext> factory)
        {
            _factory = factory;
        }

        public async Task<List<ChatMessage>> GetMessages(int conversationId)
        {
            using (var _dbContext = _factory.CreateDbContext())
            {
                return await _dbContext.ChatMessages.Where(m => m.ToConversationId == conversationId).OrderBy(m => m.DateSentUTC).ToListAsync();
            }
        }

        public async Task Create(ChatMessage message)
        {
            using (var _dbContext = _factory.CreateDbContext())
            {
                _dbContext.ChatMessages.Add(message);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
