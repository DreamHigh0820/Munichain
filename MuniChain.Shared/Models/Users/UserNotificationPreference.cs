using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Shared.Models.Users
{
    public class UserNotificationPreference
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ID { get; set; }
        [Required]
        public string? UserID { get; set; }
        public bool Deal { get; set; } = true;
        public bool Participant { get; set; } = true;
        public bool Saved { get; set; } = true;
        public bool Opened { get; set; } = true;
        public bool Sent { get; set; } = true;
        public bool Uploaded { get; set; } = true;
        public bool DocumentCommented { get; set; } = true;
        public bool Document { get; set; } = true;
        public bool DocumentParticipant { get; set; } = true;
        public bool DocumentAnnotationAdded { get; set; } = true;
        public bool DocumentAnnotationChanged { get; set; } = true;
        public bool DocumentAnnotationCommentAdded { get; set; } = true;
        public bool Signed { get; set; } = true;
        public bool Shared { get; set; } = true;
        public bool Deleted { get; set; } = true;
        public bool Commented { get; set; } = true;
        public bool Completed { get; set; } = true;
        public bool Declined { get; set; } = true;
        public bool Revoked { get; set; } = true;
        public bool Reassigned { get; set; } = true;
        public bool Expired { get; set; } = true;
        public bool Expenditure { get; set; } = true;
        public bool Performance { get; set; } = true;
        public bool Firm { get; set; } = true;
        public bool MessageBoardComment { get; set; } = true;
    }
}
