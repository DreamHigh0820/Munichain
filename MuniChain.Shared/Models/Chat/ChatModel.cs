using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Models.Chat
{
    public class ChatMessage
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string FromUserId { get; set; } = string.Empty;
        public string FromUserDisplayName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public int ToConversationId { get; set; }
        public DateTime DateSentUTC { get; set; }
    }
}
