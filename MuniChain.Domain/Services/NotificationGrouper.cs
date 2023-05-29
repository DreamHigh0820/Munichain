using PostmarkDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Services.ThirdParty;
using Shared.Models.Users;
using static Shared.Models.DealComponents.Permissions;
using Syncfusion.Blazor;
using Shared.Models.DealComponents;
using Shared.Helpers;
using Microsoft.Extensions.Logging;

namespace Domain.Services
{
    public class NotificationGrouper
    {
        PostmarkClient PostmarkClient { get; set; }
        private ILogger<NotificationGrouper> _logger { get; set; }
        public NotificationGrouper(ILogger<NotificationGrouper> logger) {
            _logger = logger;
            PostmarkClient = new PostmarkClient("63a04942-c606-42a9-9229-9889fe4ce6a8");
        }
        public async Task SendEmails()
        {
            var messages = new List<TemplatedPostmarkMessage>();
            var groupedEmails = NotificationGroupTracker.NotificationGroupTracking.GroupBy(x => x.To);

            foreach(var groupedEmail in groupedEmails)
            {
                var message = new TemplatedPostmarkMessage();

                message.From = "app@munichain.com";
                message.To = groupedEmail.Key;
                message.TemplateId = 28674989;
                var bodyStr = "";
                foreach(var str in groupedEmail)
                {
                    bodyStr += str.Body + "<br/><br/>";
                }

                message.TemplateModel = new
                {
                    Subject = $"Munichain Activity",
                    Html = bodyStr,
                    LinkText = $"View on Munichain",
                    Link = $"https://demo.munichain.com",
                };
                messages.Add(message);
            }
            try
            {
                await PostmarkClient.SendMessagesAsync(messages.ToArray());
            }
            catch (Exception e)
            {
                _logger.LogError("Error processing Notification Grouper");
                // Do not clear
                return;
            }

            NotificationGroupTracker.NotificationGroupTracking.Clear();
            _logger.LogInformation($"Successfully processed {NotificationGroupTracker.NotificationGroupTracking.Count} notifications");

        }
    }
}
