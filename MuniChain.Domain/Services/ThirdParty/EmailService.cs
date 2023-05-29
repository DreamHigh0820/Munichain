using PostmarkDotNet;
using PostmarkDotNet.Model;
using Shared.Helpers;
using Shared.Models.AppComponents;
using Shared.Models.DealComponents;
using Shared.Models.Enums;
using Shared.Models.Users;
using Syncfusion.Blazor;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static Shared.Models.DealComponents.Permissions;

namespace Domain.Services.ThirdParty
{
    public interface IEmailService
    {
        PostmarkClient PostmarkClient { get; set; }
        Task CustomFirmInput(User user, Firm firm, DealModel deal);
        Task CustomRoleInput(string createdBy, string roleName, string userId);
        Task DealModifiedNotification(User user, DealModel deal, List<string> emails, List<ConcurrencyItem> variances);
        Task DealModifiedMultipleTimesNotification(User user, DealModel deal, List<string> emails, List<ConcurrencyItem> variances);
        Task DealPublishNotification(User user, DealModel deal, List<string> emails);
        Task DealArchivedNotification(User user, DealModel deal, List<string> emails);
        Task ParticipantAddedNotification(User user, DealModel deal, List<string> emails, DealParticipant dealParticipant);
        Task ParticipantRemovedNotification(User user, DealModel deal, List<string> dealParticipants, DealParticipant dealParticipant);
        Task ParticipantModified(User user, DealModel deal, List<string> emails, List<DealParticipant> participants, Dictionary<string, List<ConcurrencyItem>> variances);
        Task ParticipantPermissionModified(User user, DealModel deal, List<string> emails, List<string> newPermissions, List<string> oldPermissions, DealParticipant dealParticipant);
        Task PerformanceChangedNotification(User user, DealModel deal, List<string> emails, List<ConcurrencyItem> changes);
        Task PerformanceTopAccountAddedNotification(User user, DealModel deal, List<string> emails, TopAccount topAccount);
        Task PerformanceTopAccountChangedNotification(User user, DealModel deal, List<string> emails, TopAccount topAccount, List<ConcurrencyItem> variances);
        Task FirmMemberAddedToFirm(User user, Firm firm, FirmMember firmMember, List<string> emails);
        Task ErrorEmail(string message, Exception ex = null, User currentUser = null, DealModel deal = null);
        Task DocUploadNotification(Document doc, User user, DealModel deal, List<string> emails);
        Task DocDeleteNotification(Document doc, User user, DealModel deal, List<string> emails);
        Task DocCommentNotification(Document doc, User user, DealModel deal, List<string> emails);
        Task DocParticipantAddedNotification(Document doc, User user, DealModel deal, List<string> emails, DealParticipant participantAdded);
        Task DocParticipantRemovedNotification(Document doc, User user, DealModel deal, List<string> emails, DealParticipant participantRemoved);
        Task DocVisibilityChangedNotification(Document doc, User user, DealModel deal, List<string> emails);
        Task DocVisibilityChangedToPublicNotification(Document doc, User user, DealModel deal, List<string> emails);
        Task DocVisibilityChangedToParticipantsNotification(Document doc, User user, DealModel deal, List<string> emails);
        Task ExpenditureAddedNotification(User user, DealModel deal, List<string> emails, DealExpenditure newDealExpenditure);
        Task ExpenditureModifiedNotification(User user, DealModel deal, List<string> emails, DealExpenditure newDealExpenditure, List<ConcurrencyItem> variances);
        Task MessageBoardCommentNotification(User user, DealModel dealInformation, List<string> emails);
        Task NewSignupNotification(User user);
    }

    public class EmailService : IEmailService
    {
        public PostmarkClient PostmarkClient { get; set; }

        public EmailService()
        {
            PostmarkClient = new PostmarkClient("63a04942-c606-42a9-9229-9889fe4ce6a8");
        }

        #region Error Email
        public async Task ErrorEmail(string message, Exception ex, User currentUser = null, DealModel deal = null)
        {
            string html;
            if (ex == null)
            {
                html = $"User {currentUser.DisplayName} with email {currentUser.Email}: {message}";
            }
            else
            {
                if (currentUser == null)
                {
                    html = $"User has encountered unexpected error: {message}.<br><br><span>{ex?.Message}</span><br><span>{ex?.StackTrace}</span><br><span>{ex?.InnerException}</span><br><span>{ex?.Data}</span><br><br>";

                }
                else
                {
                    html = $"User {currentUser.DisplayName} with email {currentUser.Email} has encountered error: {message}.<br><br><span>{ex?.Message}</span><br><span>{ex?.StackTrace}</span><br><span>{ex?.InnerException}</span><br><span>{ex?.Data}</span><br><br>";
                }

                if (deal != null)
                {
                    html += $"Deal {deal.Id}, Issuer: {deal.Issuer}, Description: {deal.Description}";
                }
            }
            // send every 5 messages
            var errormessage = new TemplatedPostmarkMessage();
            errormessage.From = "app@munichain.com";
            errormessage.TemplateId = 28674989;
            errormessage.To = "errors@munichain.com";
            errormessage.TemplateModel = new
            {
                Subject = $"ERROR ON MUNICHAIN",
                Body = $"Error message on Munichain",
                Html = html,
            };

            await PostmarkClient.SendMessageAsync(errormessage);
        }
        #endregion

        #region Custom Firm Input
        public async Task CustomFirmInput(User user, Firm firm, DealModel deal)
        {
            var message = new TemplatedPostmarkMessage();
            message.From = "app@munichain.com";
            message.TemplateId = 28674989;
            message.To = "mlieberman@munichain.com";
            message.Cc = "mgerstenfeld@munichain.com,mgagliano@munichain.com";

            message.TemplateModel = new
            {
                Subject = $"New firm inputted into Munichain",
                Body = $"{user.DisplayName ?? user.Email} has added {firm.FirmType} {firm.Id} : {firm.Name} to deal {deal.Id} : {deal.FormattedCurrencySize} {deal.Issuer}, {deal.State}",
                LinkText = $"View",
                Link = $"https://demo.munichain.com/deal/{deal.Id}",
            };

            await PostmarkClient.SendMessageAsync(message);
        }
        #endregion

        #region Document Notifications
        public async Task DocUploadNotification(Document doc, User user, DealModel deal, List<string> emails)
        {
            foreach (var email in emails)
            {
                var message = new EmailNotification();
                message.To = email;
                message.Subject = $"Document Uploaded on {deal.FormattedCurrencySize} {deal.Issuer}, {deal.State} Deal by {user.DisplayName ?? user.Email}";
                message.Body = $"{user.DisplayName ?? user.Email} has uploaded Document '{doc.Name}' on the {deal.FormattedCurrencySize} {deal.Issuer}, {deal.State} Deal.";
                NotificationGroupTracker.NotificationGroupTracking.Add(message);
            }
        }
        public async Task DocDeleteNotification(Document doc, User user, DealModel deal, List<string> emails)
        {
            foreach (var email in emails)
            {
                var message = new EmailNotification();
                message.To = email;

                message.Subject = $"Document Deleted on {deal.FormattedCurrencySize} {deal.Issuer} , {deal.State} Deal by {user.DisplayName ?? user.Email}";
                message.Body = $"{user.DisplayName ?? user.Email} has deleted Document '{doc.Name}' on the {deal.FormattedCurrencySize} {deal.Issuer} , {deal.State} Deal.";
                NotificationGroupTracker.NotificationGroupTracking.Add(message);
            }
        }

        public async Task DocCommentNotification(Document doc, User user, DealModel deal, List<string> emails)
        {
            foreach (var email in emails)
            {
                var message = new EmailNotification();
                message.To = email;

                message.Subject = $"Document Commented on {deal.FormattedCurrencySize} {deal.Issuer}, {deal.State} Deal by {user.DisplayName ?? user.Email}";
                message.Body = $"{user.DisplayName ?? user.Email} has left a note on Document '{doc.Name}' for {deal.FormattedCurrencySize} {deal.Issuer}, {deal.State} Deal.";
                NotificationGroupTracker.NotificationGroupTracking.Add(message);
            }
        }

        public async Task DocParticipantAddedNotification(Document doc, User user, DealModel deal, List<string> emails, DealParticipant participantAdded)
        {
            foreach (var email in emails)
            {
                var message = new EmailNotification();
                message.To = email;
                message.Subject = $"{user.DisplayName ?? user.Email} has added {participantAdded.DisplayName ?? participantAdded.EmailAddress} to {doc.Name} for {deal.FormattedCurrencySize} {deal.Issuer} , {deal.State} Deal";
                message.Body = $"{user.DisplayName ?? user.Email} has added {participantAdded.DisplayName ?? participantAdded.EmailAddress} to '{doc.Name}' for {deal.FormattedCurrencySize} {deal.Issuer} , {deal.State} Deal.";
                NotificationGroupTracker.NotificationGroupTracking.Add(message);
            }
        }

        public async Task DocParticipantRemovedNotification(Document doc, User user, DealModel deal, List<string> emails, DealParticipant participantRemoved)
        {
            foreach (var email in emails)
            {
                var message = new EmailNotification();
                message.To = email;
                message.Subject = $"{user.DisplayName ?? user.Email} has removed {participantRemoved.DisplayName ?? participantRemoved.EmailAddress} to {doc.Name} for {deal.FormattedCurrencySize} {deal.Issuer} , {deal.State}";
                message.Body = $"{user.DisplayName ?? user.Email} has removed {participantRemoved} to '{doc.Name}' for {deal.FormattedCurrencySize} {deal.Issuer} , {deal.State} Deal.";
                NotificationGroupTracker.NotificationGroupTracking.Add(message);
            }
        }

        public async Task DocVisibilityChangedNotification(Document doc, User user, DealModel deal, List<string> emails)
        {
            foreach (var email in emails)
            {
                var message = new EmailNotification();
                message.To = email;
                message.Subject = $"{user.DisplayName ?? user.Email} has changed the visibility for {doc.Name} for {deal.FormattedCurrencySize} {deal.Issuer}, {deal.State}";
                message.Body = $"{user.DisplayName ?? user.Email} changed the visibility to {doc.PublicDocumentViewSettings} for '{doc.Name}' for {deal.FormattedCurrencySize} {deal.Issuer}, {deal.State} Deal.";
                NotificationGroupTracker.NotificationGroupTracking.Add(message);
            }
        }

        public async Task DocVisibilityChangedToPublicNotification(Document doc, User user, DealModel deal, List<string> emails)
        {
            foreach (var email in emails)
            {
                var message = new EmailNotification();
                message.To = email;
                message.Subject = $"{user.DisplayName ?? user.Email} has uploaded {doc.Name} as a public document for {deal.FormattedCurrencySize} {deal.Issuer}, {deal.State}";
                message.Body = $"{user.DisplayName ?? user.Email} uploaded '{doc.Name}' as a public document for {deal.FormattedCurrencySize} {deal.Issuer}, {deal.State} Deal.";
                NotificationGroupTracker.NotificationGroupTracking.Add(message);
            }
        }

        public async Task DocVisibilityChangedToParticipantsNotification(Document doc, User user, DealModel deal, List<string> emails)
        {
            foreach (var email in emails)
            {
                var message = new EmailNotification();
                message.To = email;
                message.Subject = $"{user.DisplayName ?? user.Email} has uploaded {doc.Name} to all deal participants for {deal.FormattedCurrencySize} {deal.Issuer}, {deal.State}";
                message.Body = $"{user.DisplayName ?? user.Email} uploaded '{doc.Name}' to all deal participants for {deal.FormattedCurrencySize} {deal.Issuer}, {deal.State} Deal.";
                NotificationGroupTracker.NotificationGroupTracking.Add(message);
            }
        }

        #endregion

        #region ParticipantNotifications
        public async Task ParticipantAddedNotification(User user, DealModel deal, List<string> emails, DealParticipant dealParticipant)
        {
            foreach (var email in emails)
            {
                var message = new EmailNotification();
                message.To = email;
                message.Subject = $"Added to Munichain Deal {deal.FormattedCurrencySize} {deal.Issuer}, {deal.State} by {user.DisplayName ?? user.Email}";
                message.Body = $"{user.DisplayName ?? user.Email} has added {dealParticipant.DisplayName ?? dealParticipant.EmailAddress} on the {deal.FormattedCurrencySize} {deal.Issuer}, {deal.State} Deal.";
                NotificationGroupTracker.NotificationGroupTracking.Add(message);
            }
        }

        public async Task ParticipantRemovedNotification(User user, DealModel deal, List<string> dealParticipants, DealParticipant dealParticipant)
        {
            foreach (var email in dealParticipants)
            {
                var message = new EmailNotification();
                message.To = email;
                message.Subject = $"{dealParticipant.DisplayName ?? dealParticipant.EmailAddress} removed from Munichain Deal {deal.FormattedCurrencySize} {deal.Issuer}, {deal.State} by {user.DisplayName ?? user.Email}";
                message.Body = $"{user.DisplayName ?? user.Email} has removed {dealParticipant.DisplayName ?? dealParticipant.EmailAddress} as a Participant on the {deal.FormattedCurrencySize} {deal.Issuer}, {deal.State} Deal.";
                NotificationGroupTracker.NotificationGroupTracking.Add(message);
            }
        }

        public async Task ParticipantModified(User user, DealModel deal, List<string> emails, List<DealParticipant> participants, Dictionary<string, List<ConcurrencyItem>> variances)
        {
            StringBuilder smb = new StringBuilder();
            foreach (var key in variances.Keys.Distinct())
            {
                var participant = participants.FirstOrDefault(x => x.Id == key);
                foreach (var variance in variances?.Where(x => x.Key == key)?.SelectMany(x => x.Value))
                {
                    string varUpdated = variance?.Updated?.ToString();
                    if (string.IsNullOrEmpty(varUpdated))
                    {
                        varUpdated = "no input";
                    }
                    string varFromDB = variance?.FromDB?.ToString();
                    if (string.IsNullOrEmpty(varFromDB))
                    {
                        varFromDB = "no input";
                    }
                    smb.Append($"\n " +
                        $"{participant?.DisplayName ?? participant?.EmailAddress}'s {variance?.Prop} from {varUpdated} to {varFromDB}, \n");
                }
            }
            smb.Append($"\n for {deal.FormattedCurrencySize} {deal.Issuer} {deal.State} Deal.\n");
            foreach (var email in emails)
            {
                var message = new EmailNotification();
                message.To = email;
                message.Subject = $"Participant Information changed for Munichain Deal {deal.FormattedCurrencySize} {deal.Issuer} {deal.State} by {user.DisplayName ?? user.Email}";
                message.Body = smb.ToString();
                NotificationGroupTracker.NotificationGroupTracking.Add(message);
            }
        }

        public async Task ParticipantPermissionModified(User user, DealModel deal, List<string> emails, List<string> newPermissions, List<string> oldPermissions, DealParticipant dealParticipant)
        {
            StringBuilder smb = new StringBuilder();
            smb.Append($"{user.DisplayName ?? user.Email} has changed {dealParticipant.DisplayName ?? dealParticipant.EmailAddress}'s permissions: \n");
            foreach (var v in newPermissions)
            {
                smb.Append($" {oldPermissions.Where(x => x.StartsWith(v[0])).FirstOrDefault()} to {v} \n");
            }
            smb.Append($" for {deal.FormattedCurrencySize} {deal.Issuer}, {deal.State} ");

            foreach (var email in emails)
            {
                var message = new EmailNotification();
                message.To = email;
                message.Subject = $"Participant Access changed for Munichain Deal {deal.FormattedCurrencySize} {deal.Issuer}, {deal.State} by {user.DisplayName ?? user.Email}";
                message.Body = smb.ToString();
                NotificationGroupTracker.NotificationGroupTracking.Add(message);
            }
        }

        #endregion

        #region DealNotfications
        public async Task DealModifiedNotification(User user, DealModel deal, List<string> emails, List<ConcurrencyItem> variances)
        {
            string? updated = variances.Select(x => x.Updated).FirstOrDefault()?.ToString();
            if (string.IsNullOrEmpty(updated))
            {
                updated = "no input";
            }
            string? fromDB = variances.Select(x => x.FromDB).FirstOrDefault()?.ToString();
            if (string.IsNullOrEmpty(fromDB))
            {
                fromDB = "no input";
            }
            string? propertyChanged = variances.Select(x => x.Prop).FirstOrDefault()?.ToString();
            if (propertyChanged == "SaleDateUTC")
            {
                //trim the ends to remove the time for sales date changes
                if (fromDB != null)
                {
                    string? newFromDB = fromDB?.Substring(0, fromDB.Length - 12);
                    fromDB = newFromDB;
                }
                if (string.IsNullOrEmpty(fromDB))
                {
                    fromDB = "TBA";
                }
                if (updated != null)
                {
                    string? newUpdated = updated?.Substring(0, updated.Length - 12);
                    updated = newUpdated;
                }
                if (string.IsNullOrEmpty(updated))
                {
                    updated = "TBA";
                }
            }
            if (propertyChanged == "State")
            {
                int inputNum = int.Parse(updated);
                if (updated != "no input")
                {
                    updated = Enum.GetName(typeof(States), inputNum);
                };
                int dbNum = int.Parse(fromDB);
                if (fromDB != "no input")
                {
                    fromDB = Enum.GetName(typeof(States), dbNum);
                };
            }
            string body;
            ChangeEventType? eventType = variances.Select(x => x.EventType).FirstOrDefault();
            if (eventType == ChangeEventType.Added || fromDB == "no input")
            {
                body = $"{user.DisplayName ?? user.Email} has added {propertyChanged} {updated} \n";
            }
            else if (eventType == ChangeEventType.Removed || updated == "no input")
            {
                body = $"{propertyChanged} has been removed, \n";
            }
            else
                body = $"{user.DisplayName ?? user.Email} has changed {propertyChanged} from {fromDB} to {updated}";

            foreach (var email in emails)
            {
                var message = new EmailNotification();
                message.To = email;
                message.Subject = $"Deal {deal.FormattedCurrencySize} {deal.Issuer}, {deal.State} has been modified";
                message.Body = $"{body} for deal {deal.FormattedCurrencySize} {deal.Issuer}, {deal.State}";
                NotificationGroupTracker.NotificationGroupTracking.Add(message);
            }
        }

        public async Task DealModifiedMultipleTimesNotification(User user, DealModel deal, List<string> emails, List<ConcurrencyItem> variances)
        {
            var messageBuilder = new StringBuilder();
            messageBuilder.Append($"{user.DisplayName ?? user.Email} has modified the following properties: \n");
            foreach (var variance in variances)
            {
                string? fromDB = variance.FromDB?.ToString();
                string? updated = variance.Updated?.ToString();
                if (variance.Prop == "SaleDateUTC")
                {
                    //trim the ends to remove the time for sales date changes
                    if (fromDB != null)
                    {
                        string? newFromDB = fromDB?.Substring(0, fromDB.Length - 12);
                        fromDB = newFromDB;
                    }
                    if (string.IsNullOrEmpty(fromDB))
                    {
                        fromDB = "TBA";
                    }
                    if (updated != null)
                    {
                        string? newUpdated = updated?.Substring(0, updated.Length - 12);
                        updated = newUpdated;
                    }
                    if (string.IsNullOrEmpty(updated))
                    {
                        updated = "TBA";
                    }
                    messageBuilder.Append($"{variance.BaseModelName} {variance.Prop} has been changed from {fromDB} to {updated}, \n");
                }
                else if (variance.Prop == "State")
                {
                    int inputNum = int.Parse(updated);
                    if (updated != "no input")
                    {
                        updated = Enum.GetName(typeof(States), inputNum);
                    };
                    int dbNum = int.Parse(fromDB);
                    if (fromDB != "no input")
                    {
                        fromDB = Enum.GetName(typeof(States), dbNum);
                    };
                }
                else if (variance.EventType == ChangeEventType.Added)
                {
                    messageBuilder.Append($"{variance.Prop} has been added, \n");
                }
                else if (variance.EventType == ChangeEventType.Removed)
                {
                    messageBuilder.Append($"{variance.Prop} has been removed, \n");
                }
                else if (string.IsNullOrEmpty(fromDB))
                {
                    messageBuilder.Append($"{variance.BaseModelName} {variance.Prop} has been set to {updated}, \n");
                }
                else if (string.IsNullOrEmpty(updated))
                {
                    messageBuilder.Append($"{variance.BaseModelName} {variance.Prop} has been removed, \n");
                }
                else
                {
                    messageBuilder.Append($"{variance.BaseModelName} {variance.Prop} has been changed from {fromDB} to {updated}, \n");
                }
            }
            messageBuilder.Append($" for deal {deal.FormattedCurrencySize} {deal.Issuer}, {deal.State}");

            foreach (var email in emails)
            {
                var message = new EmailNotification();
                message.To = email;
                message.Subject = $"Deal {deal.FormattedCurrencySize} {deal.Issuer}, {deal.State} has been modified";
                message.Body = $"{messageBuilder}";
                NotificationGroupTracker.NotificationGroupTracking.Add(message);
            }
        }

        public async Task DealPublishNotification(User user, DealModel deal, List<string> emails)
        {
            foreach (var email in emails)
            {
                var message = new EmailNotification();
                message.To = email;
                message.Subject = $"{deal.Issuer} has been published";
                message.Body = $"{deal.FormattedCurrencySize} {deal.Issuer}, {deal.State} has been published by {user.DisplayName}. Please view the deal below.";
                NotificationGroupTracker.NotificationGroupTracking.Add(message);
            }
        }
        public async Task DealArchivedNotification(User user, DealModel deal, List<string> emails)
        {
            foreach (var email in emails)
            {
                var message = new EmailNotification();
                message.To = email;
                message.Subject = $"{deal.Issuer} has been archived";
                message.Body = $"{deal.FormattedCurrencySize} {deal.Issuer}, {deal.State} has been archived by {user.DisplayName}.";
                NotificationGroupTracker.NotificationGroupTracking.Add(message);
            }
        }
        #endregion

        #region Expenditure Notifications
        public async Task ExpenditureAddedNotification(User user, DealModel deal, List<string> emails, DealExpenditure newDealExpenditure)
        {
            foreach (var email in emails)
            {
                var message = new EmailNotification();
                message.To = email;
                message.Subject = $"Expenditure has been added to Munichain {deal.FormattedCurrencySize} {deal.Issuer}, {deal.State} by {user.DisplayName ?? user.Email}";
                message.Body = $"{user.DisplayName ?? user.Email} has added an expense of {newDealExpenditure.Value} for {newDealExpenditure.Name}";
                NotificationGroupTracker.NotificationGroupTracking.Add(message);
            }
        }

        public async Task ExpenditureModifiedNotification(User user, DealModel deal, List<string> emails, DealExpenditure newDealExpenditure, List<ConcurrencyItem> variances)
        {
            if (variances == null || variances.Count == 0)
            {
                return;
            }
            if (variances.Count == 1)
            {
                var variance = variances[0];

                foreach (var email in emails)
                {
                    var message = new EmailNotification();
                    message.To = email;
                    message.Subject = $"Expenditure has been modified on Munichain Deal {deal.FormattedCurrencySize} {deal.Issuer}, {deal.State} by {user.DisplayName ?? user.Email}";
                    message.Body = $"{user.DisplayName ?? user.Email} has modified {variance.Prop} from {variance.FromDB} to {variance.Updated} on {deal.FormattedCurrencySize} {deal.Issuer}, {deal.State} Deal.";
                    NotificationGroupTracker.NotificationGroupTracking.Add(message);
                }
            }
            else
            {
                StringBuilder bodyMessage = new StringBuilder();
                bodyMessage.Append($"{user.DisplayName ?? user.Email} has modified the following Expenditure items:");
                foreach (var variance in variances)
                {
                    bodyMessage.Append($"{variance.Prop} from {variance.FromDB} to {variance.Updated}");
                }
                bodyMessage.Append($"on {deal.FormattedCurrencySize} {deal.Issuer}, {deal.State} Deal.");
                foreach (var email in emails)
                {
                    var message = new EmailNotification();
                    message.To = email;
                    message.Subject = $"Expenditures have been modified on Munichain Deal {deal.FormattedCurrencySize} {deal.Issuer}, {deal.State} by {user.DisplayName ?? user.Email}";
                    message.Body = $"{bodyMessage}";
                    NotificationGroupTracker.NotificationGroupTracking.Add(message);
                }
            }
        }
        #endregion

        #region Performance Notification
        public async Task PerformanceChangedNotification(User user, DealModel deal, List<string> emails, List<ConcurrencyItem> changes)
        {
            if (changes.Any())
            {
                StringBuilder smb = new StringBuilder();
                if (changes.Count() == 1)
                {
                    var variance = changes[0];
                    smb.Append($"{user.DisplayName ?? user.Email} has changed {variance.Prop} from {variance.FromDB ?? "no input"} to {variance.Updated} for deal {deal.FormattedCurrencySize} {deal.Issuer}, {deal.State}. ");
                }
                else if (changes.Count() > 1)
                {

                    smb.Append($"{user.DisplayName ?? user.Email} has changed the following Performance items: ");
                    foreach (var variance in changes)
                    {
                        smb.Append($"\n " +
                                $"{variance.Prop} from {variance.FromDB ?? "no input"} to {variance.Updated}; ");
                    }
                    smb.Append($" for deal {deal.FormattedCurrencySize} {deal.Issuer}, {deal.State}. ");
                }

                foreach (var email in emails)
                {
                    var message = new EmailNotification();
                    message.To = email;
                    message.Subject = $"Performance has been modified to Munichain Deal {deal.FormattedCurrencySize} {deal.Issuer}, {deal.State} by {user.DisplayName ?? user.Email}";
                    message.Body = smb.ToString();
                    NotificationGroupTracker.NotificationGroupTracking.Add(message);
                }
            }
        }
        public async Task PerformanceTopAccountAddedNotification(User user, DealModel deal, List<string> emails, TopAccount topAccount)
        {
            foreach (var email in emails)
            {
                var message = new EmailNotification();
                message.To = email;
                message.Subject = $"{topAccount.AccountName} has been added to Munichain Deal {deal.FormattedCurrencySize} {deal.Issuer}, {deal.State} by {user.DisplayName ?? user.Email}";
                message.Body = $"{user.DisplayName ?? user.Email} has added {topAccount.AccountName ?? "a new account"} that has taken down {topAccount.ParAmount ?? 0} of the {string.Format("{0:MM/dd/yyyy}", topAccount.MaturityDateUTC) ?? "TBA"} on {deal.FormattedCurrencySize} {deal.Issuer}, {deal.State} Deal.";
                NotificationGroupTracker.NotificationGroupTracking.Add(message);
            }
        }

        public async Task PerformanceTopAccountChangedNotification(User user, DealModel deal, List<string> emails, TopAccount topAccount, List<ConcurrencyItem> variances)
        {
            if (variances.Count == 1)
            {
                ConcurrencyItem variance = variances[0];
                foreach (var email in emails)
                {
                    var message = new EmailNotification();
                    message.To = email;
                    message.Subject = $"{topAccount.AccountName} has been changed for Munichain Deal {deal.FormattedCurrencySize} {deal.Issuer}, {deal.State} by {user.DisplayName ?? user.Email}";
                    message.Body = $"{user.DisplayName ?? user.Email} has changed {topAccount.AccountName}'s {string.Format("{0:MM/dd/yyyy}", topAccount.MaturityDateUTC) ?? "TBA"} {variance.Prop} from {variance.FromDB ?? "no input"} to {variance.Updated ?? "no input"} on {deal.FormattedCurrencySize} {deal.Issuer}, {deal.State} Deal.";
                    NotificationGroupTracker.NotificationGroupTracking.Add(message);
                }
            }
            else if (variances.Count > 1)
            {
                StringBuilder bodyMessage = new StringBuilder();
                bodyMessage.Append($"{user.DisplayName ?? user.Email} has changed the following items for {topAccount.AccountName}'s {string.Format("{0:MM/dd/yyyy}", topAccount.MaturityDateUTC) ?? "TBA"}:");
                foreach (var variance in variances)
                {
                    bodyMessage.Append($"{variance.Prop} has been changed from {variance.FromDB} to {variance.Updated}; ");
                }
                bodyMessage.Append($" on {deal.FormattedCurrencySize} {deal.Issuer}, {deal.State} Deal. ");
                foreach (var email in emails)
                {
                    var message = new EmailNotification();
                    message.To = email;
                    message.Subject = $"{topAccount.AccountName} has been changed for Munichain Deal {deal.FormattedCurrencySize} {deal.Issuer}, {deal.State} by {user.DisplayName ?? user.Email}";
                    message.Body = $"{bodyMessage}";
                    NotificationGroupTracker.NotificationGroupTracking.Add(message);
                }
            }
        }
        #endregion

        #region Firm Notifications
        public async Task FirmMemberAddedToFirm(User user, Firm firm, FirmMember firmMember, List<string> emails)
        {
            foreach (var email in emails)
            {
                var message = new EmailNotification();
                message.To = email;
                message.Subject = $"{firmMember.UserId ?? firmMember.EmailAddress} has been added to {firm.Name}";
                message.Body = $"{user.DisplayName ?? user.Email} has added {firmMember.UserId ?? firmMember.EmailAddress} to {firm.Name}";
                NotificationGroupTracker.NotificationGroupTracking.Add(message);
            }
        }
        #endregion

        #region MessageBoardNotifications
        public async Task MessageBoardCommentNotification(User user, DealModel deal, List<string> emails)
        {
            foreach (var email in emails)
            {
                var message = new EmailNotification();
                message.To = email;
                message.Subject = $"New message on Deal {deal.FormattedCurrencySize} {deal.Issuer}, {deal.State}";
                message.Body = $"{user.DisplayName} has commented on {deal.FormattedCurrencySize} {deal.Issuer}, {deal.State}";
                NotificationGroupTracker.NotificationGroupTracking.Add(message);
            }
        }

        #endregion

        #region New Signup
        public async Task NewSignupNotification(User user)
        {
            var message = new TemplatedPostmarkMessage();
            message.From = "app@munichain.com";
            message.TemplateId = 28674989;
            message.To = "mlieberman@munichain.com";
            message.Cc = "mgerstenfeld@munichain.com,glondon@munichain.com,mgagliano@munichain.com";

            message.TemplateModel = new
            {
                Subject = $"New signup to Munichain",
                Body = $"{user?.Email} has signed up. Name: {user?.DisplayName}, ID: {user.Id}",
                LinkText = $"View",
                Link = $"https://demo.munichain.com/",
            };

            await PostmarkClient.SendMessageAsync(message);
        }
        #endregion

        #region Custom Inputs
        public async Task CustomRoleInput(string createdBy, string roleName, string UserId)
        {
            var message = new TemplatedPostmarkMessage();
            message.From = "app@munichain.com";
            message.TemplateId = 28674989;
            message.To = "mlieberman@munichain.com";
            message.Cc = "mgerstenfeld@munichain.com,mgagliano@munichain.com";

            message.TemplateModel = new
            {
                Subject = $"New role inputted into Munichain",
                Body = $"{createdBy} has added the role: {roleName}",
                LinkText = $"View",
                Link = $"https://demo.munichain.com/advisor/{UserId}",
            };
        }

        public async Task CustomFirmInput(string createdBy, string firmName, string firmId, FirmType? firmType, string dealId, string dealName)
        {
            var message = new TemplatedPostmarkMessage();
            message.From = "app@munichain.com";
            message.TemplateId = 28674989;
            message.To = "mlieberman@munichain.com";
            message.Cc = "mgerstenfeld@munichain.com,mgagliano@munichain.com";

            message.TemplateModel = new
            {
                Subject = $"New firm inputted into Munichain",
                Body = $"{createdBy} has added {firmType} {firmId} : {firmName} to deal {dealId} : {dealName}",
                LinkText = $"View",
                Link = $"https://demo.munichain.com/deal/{dealId}",
            };

            await PostmarkClient.SendMessageAsync(message);
        }
        #endregion
    }
}
