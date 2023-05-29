using Microsoft.AspNetCore.Components;
using Shared.Models.AppComponents;
using Shared.Models.DealComponents;
using Shared.Models.Users;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Shared.Helpers
{
    public static class NotificationExtensions
    {
        public static NotificationMarkup ToNotification(this Notification notification)
        {
            var notificationMarkup = new NotificationMarkup(notification);

            #region Document Notifications
            if (notification.Action == Models.Enums.NotificationAction.Uploaded)
            {
                notificationMarkup.Title = (MarkupString)(@" has uploaded document " + $"<a href='/document/{notification.DocumentId}'" + $" style='color:#059669;'>" + notification.DocumentName);
                notificationMarkup.SubTitle = (MarkupString)(notification.DealSize + " " + notification.DealDesc + ", " + notification.DealState);
            }
            else if (notification.Action == Models.Enums.NotificationAction.Opened)
            {
                notificationMarkup.Title = (MarkupString)(@" has opened your document " + $"<a href='/document/{notification.DocumentId}'" +
                    $" style='color:#059669;'>" + notification.DocumentName);
                notificationMarkup.SubTitle = (MarkupString)(notification.DealSize + " " + notification.DealDesc + ", " + notification.DealState);
            }
            else if (notification.Action == Models.Enums.NotificationAction.Saved)
            {
                notificationMarkup.Title = (MarkupString)(@" has saved your document " + $"<a href='/document/{notification.DocumentId}' style='color:#059669;'>" + notification.DocumentName + "</a>");
                notificationMarkup.SubTitle = (MarkupString)(notification.DealSize + " " + notification.DealDesc + ", " + notification.DealState);
            }
            else if (notification.Action == Models.Enums.NotificationAction.Sent)
            {
                notificationMarkup.Title = (MarkupString)(@" has sent you a signature document " + $"<a href='/document/{notification.DocumentId}' style='color:#059669;'>" + notification.DocumentName);
                notificationMarkup.SubTitle = (MarkupString)(notification.DealSize + " " + notification.DealDesc + ", " + notification.DealState);
            }
            else if (notification.Action == Models.Enums.NotificationAction.Revoked)
            {
                notificationMarkup.Title = (MarkupString)(@" has revoked the signature document " + $"<a href='/document/{notification.DocumentId}' style='color:#059669;'>" + notification.DocumentName);
            }
            else if (notification.Action == Models.Enums.NotificationAction.Signed)
            {
                notificationMarkup.Title = (MarkupString)(@" has signed the signature document " + $"<a href='/document/{notification.DocumentId}' style='color:#059669;'>" + notification.DocumentName);
            }
            else if (notification.Action == Models.Enums.NotificationAction.Completed)
            {
                notificationMarkup.Title = (MarkupString)(@"All signature parties have completed document " + $"<a href='/document/{notification.DocumentId}' style='color:#059669;'>" + notification.DocumentName);
            }
            else if (notification.Action == Models.Enums.NotificationAction.Expired)
            {
                notificationMarkup.Title = (MarkupString)(@"Signature document " + $"<a href='/document/{notification.DocumentId}' style='color:#059669;'>" + notification.DocumentName + "has expired.");
            }
            else if (notification.Action == Models.Enums.NotificationAction.Reassigned)
            {
                notificationMarkup.Title = (MarkupString)(@"Signature document " + $"<a href='/document/{notification.DocumentId}' style='color:#059669;'>" + notification.DocumentName + "</a> has been reassigned.");
            }
            else if (notification.Action == Models.Enums.NotificationAction.Deleted)
            {
                notificationMarkup.Title = (MarkupString)(@" has deleted document " + $"<a href='/document/{notification.DocumentId}' style='color:#059669;'>" + notification.DocumentName + "</a>");
            }
            else if (notification.Action == Models.Enums.NotificationAction.DocumentParticipantAdded)
            {
                notificationMarkup.Title = (MarkupString)(@" has added " + notification.DealParticipant + " to " + $"<a href='/document/{notification.DocumentId}' style='color:#059669;'>" + notification.DocumentName + "</a>");
            }
            else if (notification.Action == Models.Enums.NotificationAction.DocumentParticipantRemoved)
            {
                notificationMarkup.Title = (MarkupString)(@" has removed " + notification.DealParticipant + " to " + $"<a href='/document/{notification.DocumentId}' style='color:#059669;'>" + notification.DocumentName + "</a>");
            }
            else if (notification.Action == Models.Enums.NotificationAction.DocumentCommentedOn)
            {
                notificationMarkup.Title = (MarkupString)(@" has left a note on " + $"<a href='/document/{notification.DocumentId}' style='color:#059669;'>" + notification.DocumentName + "</a>");
            }
            else if (notification.Action == Models.Enums.NotificationAction.DocumentVisibilityChangedToPublic)
            {
                notificationMarkup.Title = (MarkupString)(@" has uploaded publicly viewable document" + $"<a href='/document/{notification.DocumentId}' style='color:#059669;'>" + notification.DocumentName + "</a>.");
            }
            else if (notification.Action == Models.Enums.NotificationAction.DocumentVisibilityChangedToParticipants)
            {
                notificationMarkup.Title = (MarkupString)(@" has uploaded " + $"<a href='/document/{notification.DocumentId}' style='color:#059669;'>" + notification.DocumentName + "</a> for all deal participants");
            }
            else if (notification.Action == Models.Enums.NotificationAction.DocumentVisibilityChanged)
            {
                notificationMarkup.Title = (MarkupString)(@" has changed the visibility for " + $"<a href='/document/{notification.DocumentId}' style='color:#059669;'>" + notification.DocumentName + "</a> to " + notification.NewObject);
            }
            #endregion
            #region Message Board Notifications
            else if (notification.Action == Models.Enums.NotificationAction.MessageBoardComment)
            {
                notificationMarkup.Title = (MarkupString)@$" has commented on the Message board";
            }
            #endregion
            #region Deal Notifications
            else if (notification.Action == Models.Enums.NotificationAction.DealMadePublic)
            {
                notificationMarkup.Title = (MarkupString)@" has published deal";
            }
            else if (notification.Action == Models.Enums.NotificationAction.DealArchived)
            {
                notificationMarkup.Title = (MarkupString)@" has archived deal";

            }
            else if (notification.Action == Models.Enums.NotificationAction.DealUpdated)
            {
                if (notification.PropertyChanged == "Deal SaleDateUTC")
                {
                    string originalObject;
                    if (notification.OldObject == null || notification.OldObject == " to TBA " || notification.OldObject == "")
                    {
                        originalObject = "";
                    }
                    else
                        originalObject = $" from {notification.OldObject}";

                    string newObject;
                    if (notification.NewObject == null || notification.NewObject == "")
                    {
                        newObject = $" to TBA";
                    }
                    else
                        newObject = $" to {notification.NewObject}";

                    notificationMarkup.Title = (MarkupString)(@" has updated " + notification.PropertyChanged + (originalObject) + (newObject));
                }
                else
                {
                    string originalObject;
                    if (notification.OldObject == null || notification.OldObject == "" || notification.OldObject == " to TBA ")
                    {
                        originalObject = "";
                    }
                    else
                        originalObject = $" from {notification.OldObject}";

                    string newObject;
                    if (notification.NewObject == null || notification.NewObject == "")
                    {
                        newObject = "";
                    }
                    else
                        newObject = $" to {notification.NewObject}";

                    if (newObject == "")
                    {
                        notificationMarkup.Title = (MarkupString)(@" has removed " + notification.PropertyChanged);
                    }
                    else
                        notificationMarkup.Title = (MarkupString)(@" has updated " + notification.PropertyChanged + originalObject + newObject);
                }
            }
            else if (notification.Action == Models.Enums.NotificationAction.DealUpdatedMultiple)
            {
                notificationMarkup.Title = (MarkupString)notification.NewObject;
            }
            else if (notification.Action == Models.Enums.NotificationAction.DealPartAdded)
            {
                notificationMarkup.Title = (MarkupString)(@" has added " + notification.PropertyChanged);

            }
            else if (notification.Action == Models.Enums.NotificationAction.DealPartRemoved)
            {
                notificationMarkup.Title = (MarkupString)(@" has removed " + notification.PropertyChanged);
            }
            #endregion
            #region Participants
            else if (notification.Action == Models.Enums.NotificationAction.ParticipantAdd)
            {
                notificationMarkup.Title = (MarkupString)(@" has added " + notification.DealParticipant + " as a " + notification.DealRole);
            }
            else if (notification.Action == Models.Enums.NotificationAction.ParticipantRemove)
            {
                notificationMarkup.Title = (MarkupString)(@" has removed " + notification.DealParticipant);

            }
            else if (notification.Action == Models.Enums.NotificationAction.ParticipantRoleUpdated)
            {
                notificationMarkup.Title = (MarkupString)(@" has changed " + notification.DealParticipant + "'s role from " + (notification.OldRole ?? "no role") + " to " + (notification.DealRole ?? "no role"));
            }
            else if (notification.Action == Models.Enums.NotificationAction.ParticipantPublic)
            {
                notificationMarkup.Title = (MarkupString)(@" has set " + notification.DealParticipant + " to show publicly");

            }
            else if (notification.Action == Models.Enums.NotificationAction.ParticipantPrivate)
            {
                notificationMarkup.Title = (MarkupString)(@" has removed " + notification.DealParticipant + " from showing publicly");
            }
            else if (notification.Action == Models.Enums.NotificationAction.ParticipantModified)
            {
                notificationMarkup.Title = (MarkupString)(@" has modified " + notification.DealParticipant + " " + notification.PropertyChanged + " from " + notification.OldObject + " to " + notification.NewObject);
            }
            else if (notification.Action == Models.Enums.NotificationAction.ParticipantPermissionUpdate)
            {
                notificationMarkup.Title = (MarkupString)(@" has changed " + notification.DealParticipant + " " + notification.PropertyChanged + " from " + notification.OldObject + " to " + notification.NewObject);
            }
            else if (notification.Action == Models.Enums.NotificationAction.ParticipantAddedAsAdmin)
            {
                notificationMarkup.Title = (MarkupString)(@" has changed " + notification.DealParticipant + " to a Deal Admin");
            }
            #endregion
            #region Expenditure Notifications
            else if (notification.Action == Models.Enums.NotificationAction.ExpenditureAdded)
            {
                notificationMarkup.Title = (MarkupString)(@" has added an expense of " + (notification.ExpenditureValue ?? "0") + " for " + notification.ExpenditureField);
            }
            else if (notification.Action == Models.Enums.NotificationAction.ExpenditureChanged)
            {
                notificationMarkup.Title = (MarkupString)(@" has changed " + notification.PropertyChanged + " expenditure from $" + (notification.OldObject ?? "0") + " to $" + (notification.NewObject ?? "0"));
            }
            else if (notification.Action == Models.Enums.NotificationAction.ExpenditureMultipleChanges)
            {
                notificationMarkup.Title = (MarkupString)(@" has changed " + notification.PropertyChanged + " expenditure from $" + (notification.OldObject ?? "0") + " to $" + (notification.NewObject ?? "0"));
            }
            #endregion
            #region Performance Notifications
            else if (notification.Action == Models.Enums.NotificationAction.PerformanceAdded)
            {
                notificationMarkup.Title = (MarkupString)(@" has set " + notification.PropertyChanged + " to " + (notification.NewObject ?? "0"));

            }
            else if (notification.Action == Models.Enums.NotificationAction.PerformanceUpdated)
            {
                notificationMarkup.Title = (MarkupString)(@" has changed " + notification.PropertyChanged + " from " + (notification.OldObject ?? "0") + " to " + notification.NewObject);
            }
            else if (notification.Action == Models.Enums.NotificationAction.PerformanceTopAccountAdded)
            {
                notificationMarkup.Title = (MarkupString)(@" has added " + notification.TopAccount + " that has taken down " + (notification.TopAccountParAmount ?? "0") + " of the " + notification.TopAccountMaturityDateUTC + " maturity");
            }
            else if (notification.Action == Models.Enums.NotificationAction.PerformanceTopAccountChanged)
            {
                notificationMarkup.Title = (MarkupString)(@" has changed " + notification.TopAccount + "'s " + notification.PropertyChanged + " from " + notification.OldObject + " to " + notification.NewObject);
            }
            else if (notification.Action == Models.Enums.NotificationAction.PerformanceTopAccountMarginAmountChanged)
            {
                notificationMarkup.Title = (MarkupString)(@" has changed " + notification.TopAccount + "'s " + notification.TopAccountMaturityDateUTC.Value.Date + " maturity amount from $" + notification.OldObject + " to $" + notification.NewObject);
            }
            else if (notification.Action == Models.Enums.NotificationAction.PerformanceTopAccountMarginDateChanged)
            {
                notificationMarkup.Title = (MarkupString)(@" has changed " + notification.TopAccount + "'s " + notification.TopAccountMaturityDateUTC.Value.Date + " maturity date from " + notification.OldObject + " to " + notification.NewObject);
            }
            #endregion
            #region Firm Notifications
            else if (notification.Action == Models.Enums.NotificationAction.FirmMemberAddedToFirm)
            {
                notificationMarkup.Title = (MarkupString)(@" has approved " + notification.FirmMember + " to join " + notification.FirmName);
            }
            #endregion
            else
            {
                return null;
            }

            return notificationMarkup;
        }

        public static string GetDealClassDisplayName(string prop, ConcurrencyItem variance)
        {
            try
            {
                MemberInfo property;
                if (variance.BaseModelName == "Deal")
                {
                    property = typeof(DealModel).GetProperty(prop);
                    if (property == null)
                    {
                        return prop;
                    }
                }
                else if (variance.Prop.Contains("Maturity"))
                {
                    property = typeof(Maturity).GetProperty(prop);
                    if (property == null)
                    {
                        return prop;
                    }
                }
                else
                    property = typeof(Series).GetProperty(prop);
                if (property == null)
                {
                    return prop;
                }

                var attribute = property.GetCustomAttributes(typeof(DisplayNameAttribute), true).Cast<DisplayNameAttribute>().Single();
                string displayName = attribute.DisplayName ?? prop;
                return displayName;

            }
            catch (Exception)
            {
                return prop;
            }
        }

        public static string GetPerformanceClassDisplayName(string prop)
        {
            try
            {
                MemberInfo property = typeof(Performance).GetProperty(prop);
                if (property == null)
                {
                    return prop;
                }
                var attribute = property.GetCustomAttributes(typeof(DisplayNameAttribute), true).Cast<DisplayNameAttribute>().Single();
                string displayName = attribute.DisplayName ?? prop;
                return displayName;

            }
            catch (Exception)
            {
                return prop;
            }
        }
    }
}
