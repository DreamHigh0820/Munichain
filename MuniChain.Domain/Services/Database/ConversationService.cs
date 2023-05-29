using Data.DatabaseServices;
using Microsoft.EntityFrameworkCore;
using Shared.Models.Chat;

namespace Domain.Services.Database
{
    public interface IConversationService
    {
        Task<int> Create(Conversation conversation);
        Task<List<Conversation>> GetConversations(string userEmail);
        Task UpdateConversationReadStatus(int conversationId, string conversationReadByMembers);
    }

    public class ConversationService : IConversationService
    {
        private readonly IDbContextFactory<SqlDbContext> _factory;

        public ConversationService(IDbContextFactory<SqlDbContext> factory)
        {
            _factory = factory;
        }

        public async Task<List<Conversation>> GetConversations(string userEmail)
        {
            using (var _dbContext = _factory.CreateDbContext())
            {
                List<Conversation> conversations = await _dbContext.Conversations.Where(c => c.MemberEmails.Contains(userEmail)).ToListAsync();

                foreach (var conversation in conversations)
                {
                    ChatMessage lastMessage = await _dbContext.ChatMessages.Where(m => m.ToConversationId == conversation.Id)
                        .OrderByDescending(m => m.DateSentUTC).FirstOrDefaultAsync();
                    if (lastMessage != null)
                    {
                        conversation.RecentActivityTimeUTC = lastMessage.DateSentUTC;
                    }
                }

                conversations.Sort((a, b) =>
                {
                    if (a.RecentActivityTimeUTC < b.RecentActivityTimeUTC)
                    {
                        return 1;
                    }
                    else
                    {
                        return -1;
                    }
                });
                return conversations;
            }
        }

        public async Task UpdateConversationReadStatus(int conversationId, string conversationReadByMembers)
        {
            using (var _dbContext = _factory.CreateDbContext())
            {
                var conversation = await _dbContext.Conversations.FirstAsync(c => c.Id == conversationId);
                conversation.ConversationReadByMembers = conversationReadByMembers;
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<int> Create(Conversation conversation)
        {
            using (var _dbContext = _factory.CreateDbContext())
            {
                _dbContext.Conversations.Add(conversation);
                await _dbContext.SaveChangesAsync();

                return conversation.Id;
            }
        }
    }
}
