using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Models.Chat
{
    public class Conversation
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string MemberIds { get; set; } = string.Empty;
        public string MemberEmails { get; set; } = string.Empty;
        public string MemberDisplayNames { get; set; } = string.Empty;
        public DateTime DateCreatedUTC { get; set; }
        public string ConversationReadByMembers { get; set; } = string.Empty;
        [NotMapped] public DateTime RecentActivityTimeUTC { get; set; }
    }
}
