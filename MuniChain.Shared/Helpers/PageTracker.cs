using Shared.Models.DealComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Helpers
{
    public static class PageTracker
    {
        public static Dictionary<string, string> PageTracking { get; set; } = new();

    }

    public static class NotificationGroupTracker
    {
        public static List<EmailNotification> NotificationGroupTracking { get; set; } = new();

    }
}
