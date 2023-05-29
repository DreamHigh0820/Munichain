using Shared.Helpers;
using Shared.Models.DealComponents;
using Shared.Models.Enums;
using Shared.Models.Users;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;

namespace Shared.Models.AppComponents
{
    public class Notification
    {
        public Notification() { }
        public Notification(Document doc, User user, DealModel deal, string email)
        {
            Id = Guid.NewGuid().ToString();
            ActionBy = user?.DisplayName;
            if (!string.IsNullOrEmpty(email))
            {
                NotifyTo = email;
            }
            ActionDateTimeUTC = DateTime.UtcNow;
            IsUserRead = false;

            // Deal
            if (deal != null)
            {
                DealId = deal?.Id;
                DealDesc = deal?.Issuer;
                DealSize = string.Format("{0:C}", deal?.Size);
                DealState = deal?.State?.ToString();
            }

            if (doc != null)
            {
                DocumentId = doc.Id;
                DocumentName = doc.Name;
            }
        }
        
        // Base
        public string? Id { get; set; }
        public string? ActionBy { get; set; }
        public string? NotifyTo { get; set; }
        public NotificationAction? Action { get; set; } // Set outside constructor
        public DateTime? ActionDateTimeUTC { get; set; }
        public bool IsUserRead { get; set; }


        // Concurrency && History
        public string? PropertyChanged { get; set; }
        public string? OldObject { get; set; }
        public string? NewObject { get; set; }

        // Deal
        public string? DealId { get; set; }
        public string? DealDesc { get; set; }
        public string? DealSize { get; set; }
        public string? DealState { get; set; }
        public string? DealParticipant { get; set; }
        public string? DealRole { get; set; }

        // Participants
        public string? OldRole { get; set; }

        // Documents
        public string? DocumentId { get; set; }
        public string? DocumentName { get; set; }

        // Expenditures
        public string? ExpenditureField { get; set; }
        public string? ExpenditureValue { get; set; }

        // Performance
        public string? TopAccount { get; set; }
        public string? TopAccountParAmount { get; set; }
        public DateTime? TopAccountMaturityDateUTC { get; set; }

        // Firm
        public string? FirmName { get; set; }
        public string? FirmMember { get; set; }
    }
}