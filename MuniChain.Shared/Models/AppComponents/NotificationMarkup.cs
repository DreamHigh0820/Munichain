using Microsoft.AspNetCore.Components;

namespace Shared.Models.AppComponents
{
    public class NotificationMarkup
    {
        public NotificationMarkup(Notification notification)
        {
            if (notification == null)
            {
                throw new ArgumentNullException(nameof(notification));
            }

            Id = notification.Id;
            ActionBy = notification.ActionBy;
            DateTimeUTC = notification.ActionDateTimeUTC;
            IsRead = notification.IsUserRead;
            SubTitle = (MarkupString)($"<a href='/deal/{notification.DealId}' style='color:#059669;'>" + notification.DealSize + " " + notification.DealDesc + ", " + notification.DealState ?? "No State" + "</a>");
        }

        public string Id { get; set; }
        public string ActionBy { get; set; }
        public MarkupString Title { get; set; }
        public MarkupString SubTitle { get; set; }
        public DateTime? DateTimeUTC { get; set; }
        public bool IsRead { get; set; }
    }
}
