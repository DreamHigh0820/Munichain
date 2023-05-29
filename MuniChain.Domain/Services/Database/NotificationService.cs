using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Shared.Models.DealComponents;
using Shared.Helpers;
using Shared.Models.AppComponents;
using Shared.Models.Enums;
using Shared.Models.Users;
using Data.DatabaseServices;
using Domain.Services.ThirdParty;
using ICSharpCode.SharpZipLib.Core;
using System;
using System.Text;
using Newtonsoft.Json.Linq;
using Syncfusion.Blazor;
using static Shared.Models.DealComponents.Permissions;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.X509Certificates;

namespace Domain.Services.Database
{
    public interface INotificationService
    {
        Task<bool> CreateAll(List<Notification> notifications);
        Task<bool> UpdateAll(List<Notification> notifications);
        Task<bool> IsNotificationsPendingToRead(User user);
        Task<bool> Create(Notification notification);
        Task<List<Notification>> GetNotifications(string email);
        Task<List<Notification>> Search(Expression<Func<Notification, bool>> predicate = null);
        Task DealModifiedNotification(User user, DealModel deal, List<ConcurrencyItem> variances);
        Task DealArchivedNotification(DealModel deal, User user);
        Task DealPublishNotification(User user, DealModel deal);
        Task DocUploadNotification(Document docToSave, User user, DealModel deal);
        Task DocDeleteNotification(Document docToDelete, User user, DealModel deal);
        Task DocVisibilityChangedNotification(Document doc, User user, DealModel deal, PublicDocumentViewSettings? newVisibility);
        Task ParticipantAddedNotification(User user, DealModel deal, DealParticipant userAdded);
        Task ParticipantRemovedNotification(User user, DealModel deal, DealParticipant userRemoved);
        Task ParticipantModified(User user, DealModel deal, List<DealParticipant> participants, Dictionary<string, List<ConcurrencyItem>> variances);
        Task ParticipantPermissionsModified(User user, DealModel deal, List<string> newPermissions, List<string> oldPermissions, DealParticipant dealParticipant);
        Task ExpenditureAddedNotification(User user, DealModel deal, DealExpenditure newDealExpenditure);
        Task ExpendituresModifiedNotification(User user, DealModel deal, List<ConcurrencyItem> diffs, DealExpenditure dealExpenditure);
        Task PerformanceChangedNotification(User user, DealModel deal, List<ConcurrencyItem> variances);
        Task PerformanceAccountChangedNotification(User user, DealModel deal, TopAccount topAccount, bool newAccount, List<ConcurrencyItem> variance);
        Task FirmMemberAddedToFirmNotification(User user, Firm firm, FirmMember firmMember);
        Task MessageBoardCommentNotification(User user, DealModel dealInformation);
    }

    public class NotificationService : INotificationService
    {
        private readonly IDbContextFactory<SqlDbContext> _factory;
        private readonly IDealParticipantService _dealParticipantService;
        private readonly IEmailService _emailService;
        private readonly IUserService _userService;

        public NotificationService(IDbContextFactory<SqlDbContext> factory, IDealParticipantService dealParticipantService, IEmailService emailService, IUserService userService)
        {
            _factory = factory;
            _dealParticipantService = dealParticipantService;
            _emailService = emailService;
            _userService = userService;
        }

        public async Task<List<Notification>> Search(Expression<Func<Notification, bool>> predicate = null)
        {
            using (var _dbContext = _factory.CreateDbContext())
            {
                return await _dbContext.Notifications.Where(predicate)
                .OrderByDescending(x => x.ActionDateTimeUTC)
                .Take(5)
                .ToListAsync();
            }
        }

        public async Task<List<Notification>> GetNotifications(string email)
        {
            using (var _dbContext = _factory.CreateDbContext())
            {
                return await _dbContext.Notifications.Where(x => x.NotifyTo == email)
                .OrderByDescending(x => x.ActionDateTimeUTC)
                .Take(50)
                .ToListAsync();
            }
        }
        public async Task<bool> IsNotificationsPendingToRead(User user)
        {
            if (user == null)
            {
                return false;
            }

            using (var _dbContext = _factory.CreateDbContext())
            {
                try
                {
                    var notifs = _dbContext.Notifications.Where(x => x.NotifyTo == user.Email && x.IsUserRead == false && x.ActionBy != user.DisplayName);
                    return await notifs.AnyAsync();
                }
                catch (Exception)
                {
                    Console.WriteLine("ERROR");
                    return false;
                }
            }
        }
        #region Create & CreateAll
        public async Task<bool> Create(Notification notification)
        {
            using (var _dbContext = _factory.CreateDbContext())
            {
                _dbContext.Add(notification);
                try
                {
                    await _dbContext.SaveChangesAsync();
                    return true;
                }
                catch (Exception)
                {
                    Console.WriteLine("ERROR");
                    return false;
                }
            }
        }

        public async Task<bool> CreateAll(List<Notification> notifications)
        {
            notifications.ForEach(x => x.IsUserRead = false);
            using (var _dbContext = _factory.CreateDbContext())
            {
                _dbContext.AddRange(notifications);
                try
                {
                    await _dbContext.SaveChangesAsync();
                    return true;
                }
                catch (Exception)
                {
                    Console.WriteLine("ERROR");
                    return false;
                }
            }
        }
        #endregion
        #region Update All
        public async Task<bool> UpdateAll(List<Notification> notifications)
        {
            using (var _dbContext = _factory.CreateDbContext())
            {
                _dbContext.UpdateRange(notifications);
                try
                {
                    await _dbContext.SaveChangesAsync();
                    return true;
                }
                catch (Exception)
                {
                    Console.WriteLine("ERROR");
                    return false;
                }
            }
        }
        #endregion

        #region Deal Notifications
        public async Task DealModifiedNotification(User user, DealModel deal, List<ConcurrencyItem> variances)
        {
            var participants = await _dealParticipantService.GetParticipantsByDealId(deal.Id);
            var emails = participants.Where(x => x.EmailAddress != user.Email && x.DealPermissions.CanReadDeal())
                .Select(x => x.EmailAddress)
                .ToList();

            if (participants.Any())
            {
                if (variances.Count() == 1)
                {
                    List<User> users = await _userService.BatchGetUsersByEmail(emails?.ToArray());
                    var variance = variances.First();

                    foreach (var email in emails)
                    {
                        var userByEmail = users.FirstOrDefault(x => x.Email == email);
                        string? inputUpdated = variances?.Select(x => x.Updated)?.FirstOrDefault()?.ToString();
                        if (string.IsNullOrEmpty(inputUpdated))
                        {
                            inputUpdated = "";
                        }

                        string? inputFromDB = variances?.Select(x => x.FromDB)?.FirstOrDefault()?.ToString();
                        if (string.IsNullOrEmpty(inputFromDB))
                        {
                            inputFromDB = "";
                        }

                        string? inputProp = variances?.Select(x => x.Prop)?.FirstOrDefault()?.ToString();
                        if (inputProp == "State")
                        {
                            int inputNum = int.Parse(inputUpdated);
                            if (inputUpdated != "")
                            {
                                inputUpdated = Enum.GetName(typeof(States), inputNum);
                            };
                            int dbNum = int.Parse(inputFromDB);
                            if (inputFromDB != "")
                            {
                                inputFromDB = Enum.GetName(typeof(States), dbNum);
                            };
                        }
                        if (inputProp.Contains("Date"))
                        {
                            //trim the ends to remove the time for sales date changes
                            if (inputUpdated != "")
                            {
                                inputUpdated = inputUpdated.Substring(0, inputUpdated.Length - 12);
                            };
                            if (inputFromDB != "")
                            {
                                inputFromDB = inputFromDB.Substring(0, inputFromDB.Length - 12);
                            };
                        }
                        ChangeEventType? eventType = variance.EventType;
                        inputProp = NotificationExtensions.GetDealClassDisplayName(inputProp, variance);
                        if (eventType == ChangeEventType.Added)
                        {
                            Notification notification = new Notification(null, user, deal, email)
                            {
                                Action = NotificationAction.DealPartAdded,
                                PropertyChanged = variance.BaseModelName + " " + inputProp
                            };
                            await Create(notification);
                            if (!await _userService.IsNotificationRegistered(userByEmail?.Id, NotificationAction.DealPartAdded))
                            {
                                emails.Remove(email);
                            }

                        }
                        else if (eventType == ChangeEventType.Removed)
                        {
                            Notification notification = new Notification(null, user, deal, email)
                            {
                                Action = NotificationAction.DealPartRemoved,
                                PropertyChanged = variance.BaseModelName + " " + inputProp,
                            };
                            await Create(notification);
                            if (!await _userService.IsNotificationRegistered(userByEmail?.Id, NotificationAction.DealPartRemoved))
                            {
                                emails.Remove(email);
                            }
                        }
                        else
                        {
                            Notification notification = new Notification(null, user, deal, email)
                            {
                                Action = NotificationAction.DealUpdated,
                                OldObject = inputFromDB,
                                NewObject = inputUpdated,
                                PropertyChanged = variance.BaseModelName + " " + inputProp,
                            };
                            await Create(notification);

                            if (!await _userService.IsNotificationRegistered(userByEmail?.Id, NotificationAction.DealUpdated))
                            {
                                emails.Remove(email);
                            }
                        }
                    }
                    await _emailService.DealModifiedNotification(user, deal, emails, variances);
                    return;
                }
                else if (variances.Count > 1)
                {
                    StringBuilder message = new StringBuilder();
                    message.Append($" has changed the following properties: \n");
                    foreach (var variance in variances)
                    {
                        string? property = variance.Prop?.ToString();
                        if (!String.IsNullOrEmpty(property))
                        {
                            var inputProp = NotificationExtensions.GetDealClassDisplayName(property, variance);

                            ChangeEventType? eventType = variance.EventType;
                            if (eventType == ChangeEventType.Added)
                            {
                                message.Append($" added {inputProp};  \n");
                            }
                            else
                            if (eventType == ChangeEventType.Removed)
                            {
                                message.Append($" removed {inputProp};  \n");
                            }
                            else
                            {
                                string? inputUpdated = variance?.Updated?.ToString();
                                if (string.IsNullOrEmpty(variance.Updated?.ToString()))
                                {
                                    inputUpdated = "";
                                }

                                string? inputFromDB = variance?.FromDB?.ToString();
                                if (string.IsNullOrEmpty(variance.FromDB?.ToString()))
                                {
                                    inputFromDB = "";
                                }

                                if (inputProp.Contains("Date"))
                                {
                                    if (inputUpdated == "")
                                    {
                                        inputUpdated = $" TBA";
                                    }
                                    else
                                        inputUpdated = $" {inputUpdated.Substring(0, inputUpdated.Length - 12)}";

                                    if (inputFromDB == "")
                                    {
                                        inputFromDB = " from TBA ";
                                    }
                                    else
                                        inputFromDB = $" from {inputFromDB.Substring(0, inputFromDB.Length - 12)}";
                                }

                                if (variance.Prop == "State")
                                {
                                    int inputNum = int.Parse(inputUpdated);
                                    if (inputUpdated != "")
                                    {
                                        inputUpdated = Enum.GetName(typeof(States), inputNum);
                                    }
                                    else
                                        inputUpdated = "";
                                    int dbNum = int.Parse(inputFromDB);
                                    if (inputFromDB != "")
                                    {
                                        inputFromDB = " from " + Enum.GetName(typeof(States), dbNum);
                                    }
                                    else
                                        inputFromDB = "";
                                }

                                if (inputFromDB != "" && inputUpdated == "")
                                {
                                    message.Append($" {variance.BaseModelName} {inputProp} removed.");
                                }
                                else
                                    message.Append($" {variance.BaseModelName} {inputProp} changed from {inputFromDB} to {inputUpdated}; \n");

                                //create notifications for each edit for audit purposes, these will not create notification alerts
                                foreach (var email in emails)
                                {
                                    Notification notification = new Notification(null, user, deal, email)
                                    {
                                        Action = NotificationAction.DealUpdatedAudit,
                                        OldObject = inputFromDB,
                                        NewObject = inputUpdated,
                                        PropertyChanged = inputProp,
                                    };
                                    await Create(notification);
                                }
                            }
                        }
                    }

                    List<User> users = await _userService.BatchGetUsersByEmail(emails.ToArray());

                    foreach (var email in emails)
                    {
                        var userByEmail = users.FirstOrDefault(x => x.Email == email);

                        Notification notification = new Notification(null, user, deal, email)
                        {
                            Action = NotificationAction.DealUpdatedMultiple,
                            NewObject = message.ToString(),
                        };
                        await Create(notification);
                        if (!await _userService.IsNotificationRegistered(userByEmail?.Id, NotificationAction.DealUpdatedMultiple))
                        {
                            emails.Remove(email);
                        }
                    }
                    await _emailService.DealModifiedMultipleTimesNotification(user, deal, emails, variances);
                    return;
                }
            };
        }

        public async Task DealArchivedNotification(DealModel deal, User user)
        {
            var dealParticipants = await _dealParticipantService.GetParticipantsByDealId(deal.Id);
            List<string> emails = dealParticipants.Where(x => x.EmailAddress != user.Email)
                .Select(x => x.EmailAddress)
                .ToList();

            // Notify all participants of archive deal
            if (emails != null && emails.Any())
            {
                var notifications = new List<Notification>();
                List<User> users = await _userService.BatchGetUsersByEmail(emails.ToArray());

                foreach (var email in emails)
                {
                    var userByEmail = users.FirstOrDefault(x => x.Email == email);

                    notifications.Add(new Notification(null, user, deal, email)
                    {
                        Action = NotificationAction.DealArchived,
                    });

                    if (!await _userService.IsNotificationRegistered(userByEmail?.Id, NotificationAction.DealArchived))
                    {
                        emails.Remove(email);
                    }
                }
                await _emailService.DealArchivedNotification(user, deal, emails);
                await CreateAll(notifications);
            }
        }
        public async Task DealPublishNotification(User user, DealModel deal)
        {
            var dealParticipants = await _dealParticipantService.GetParticipantsByDealId(deal.Id);
            List<string> emails = dealParticipants.Where(x => x.EmailAddress != user.Email)
                .Select(x => x.EmailAddress)
                .ToList();


            if (emails != null && emails.Any())
            {
                List<Notification> notifications = new List<Notification>();
                List<User> users = await _userService.BatchGetUsersByEmail(emails.ToArray());
                foreach (var email in emails)
                {
                    var userByEmail = users.FirstOrDefault(x => x.Email == email);

                    notifications.Add(new Notification(null, user, deal, email)
                    {
                        Action = NotificationAction.DealMadePublic,
                    });
                    if (!await _userService.IsNotificationRegistered(userByEmail?.Id, NotificationAction.DealArchived))
                    {
                        emails.Remove(email);
                    }
                }
                await _emailService.DealPublishNotification(user, deal, emails);
                await CreateAll(notifications);
            }
        }
        #endregion
        #region Document Notifications
        public async Task DocUploadNotification(Document doc, User user, DealModel deal)
        {
            var participants = await _dealParticipantService.GetParticipantsByDealId(deal.Id);
            List<string>? emails = new List<string>();
            if (doc.PublicDocumentViewSettings == PublicDocumentViewSettings.Custom)
            {
                emails = participants?.Where(x => doc.UserPermissions.Contains(x.EmailAddress))?.Select(x => x.EmailAddress)?.ToList();
            }
            else if (doc.PublicDocumentViewSettings == PublicDocumentViewSettings.Participants || doc.PublicDocumentViewSettings == PublicDocumentViewSettings.Public)
            {
                emails = participants?.Select(x => x.EmailAddress).ToList();
            }
            List<Notification> notifications = new List<Notification>();
            List<User> users = await _userService.BatchGetUsersByEmail(emails?.ToArray());

            foreach (var email in emails)
            {
                notifications.Add(new Notification(doc, user, deal, email)
                {
                    Action = NotificationAction.Uploaded,
                });

                var userByEmail = users.FirstOrDefault(x => x.Email == email);

                if (!await _userService.IsNotificationRegistered(userByEmail?.Id, NotificationAction.Uploaded))
                {
                    emails.Remove(email);
                }
            }

            await _emailService.DocUploadNotification(doc, user, deal, emails);
            await CreateAll(notifications);
        }

        public async Task DocDeleteNotification(Document doc, User user, DealModel deal)
        {
            var dealParticipants = await _dealParticipantService.GetParticipantsByDealId(deal.Id);
            var emails = new List<string>();
            if (doc.PublicDocumentViewSettings == PublicDocumentViewSettings.Custom)
            {
                emails = dealParticipants?.Where(x => doc.UserPermissions.Contains(x.EmailAddress)).Select(x => x.EmailAddress).ToList();
            }
            else if (doc.PublicDocumentViewSettings == PublicDocumentViewSettings.Participants || doc.PublicDocumentViewSettings == PublicDocumentViewSettings.Public)
            {
                emails = dealParticipants?.Select(x => x.EmailAddress).ToList();
            }
            List<Notification> notifications = new List<Notification>();
            List<User> users = await _userService.BatchGetUsersByEmail(dealParticipants?.Select(x => x.EmailAddress)?.ToArray());
            foreach (var dealParticipant in dealParticipants)
            {
                notifications.Add(new Notification(doc, user, deal, dealParticipant?.EmailAddress)
                {
                    Action = NotificationAction.Deleted
                });

                var userByEmail = users.FirstOrDefault(x => x.Email == dealParticipant.EmailAddress);

                if (!await _userService.IsNotificationRegistered(dealParticipant.Id, NotificationAction.Deleted))
                {
                    emails?.Remove(dealParticipant?.EmailAddress?.ToString());
                }
            }
            await CreateAll(notifications);
            await _emailService.DocDeleteNotification(doc, user, deal, emails);
        }

        public async Task DocVisibilityChangedNotification(Document doc, User user, DealModel deal, PublicDocumentViewSettings? newVisibility)
        {
            List<DealParticipant> participants = await _dealParticipantService.GetParticipantsByDealId(deal.Id);
            List<string> emails = participants?.Where(x => x.DealPermissions.IsOnlyDealAdmin() || x.UserId == doc.CreatedBy)?.Select(x => x.EmailAddress)?.ToList();
            List<Notification> notifications = new List<Notification>();
            List<User> users = await _userService.BatchGetUsersByEmail(emails.ToArray());

            if (newVisibility == PublicDocumentViewSettings.Public)
            {
                foreach (var email in emails)
                {
                    notifications.Add(new Notification(doc, user, deal, email)
                    {
                        Action = NotificationAction.DocumentVisibilityChangedToPublic,
                    });
                    var userByEmail = users.FirstOrDefault(x => x.Email == email);
                    if (!await _userService.IsNotificationRegistered(userByEmail?.Id, NotificationAction.DocumentVisibilityChangedToPublic))
                    {
                        emails.Remove(email);
                    }
                }
                await _emailService.DocVisibilityChangedToPublicNotification(doc, user, deal, emails);
                await CreateAll(notifications);
                return;
            }
            else if (newVisibility == PublicDocumentViewSettings.Participants)
            {
                foreach (var email in emails)
                {
                    notifications.Add(new Notification(doc, user, deal, email)
                    {
                        Action = NotificationAction.DocumentVisibilityChangedToParticipants,
                    });
                    var userByEmail = users.FirstOrDefault(x => x.Email == email);
                    if (!await _userService.IsNotificationRegistered(userByEmail?.Id, NotificationAction.DocumentVisibilityChangedToParticipants))
                    {
                        emails.Remove(email);
                    }
                }
                await _emailService.DocVisibilityChangedToParticipantsNotification(doc, user, deal, emails);
                await CreateAll(notifications);
                return;
            }
            else
            {
                foreach (var email in emails)
                {
                    notifications.Add(new Notification(doc, user, deal, email)
                    {
                        Action = NotificationAction.DocumentVisibilityChanged,
                        NewObject = doc.PublicDocumentViewSettings?.ToString(),
                    });
                    var userByEmail = users.FirstOrDefault(x => x.Email == email);
                    if (!await _userService.IsNotificationRegistered(userByEmail?.Id, NotificationAction.DocumentVisibilityChanged))
                    {
                        emails.Remove(email);
                    }
                }
                await _emailService.DocVisibilityChangedNotification(doc, user, deal, emails);
                await CreateAll(notifications);
                return;
            }
        }
        #endregion
        #region Participant Notifications
        public async Task ParticipantAddedNotification(User user, DealModel deal, DealParticipant userAdded)
        {
            var participants = await _dealParticipantService.GetParticipantsByDealId(deal.Id);
            List<string> emails = participants.Where(x => x.EmailAddress != user.Email)
                .Select(x => x.EmailAddress)
                .ToList();

            if (emails.Any())
            {
                var notifications = new List<Notification>();
                List<User> users = await _userService.BatchGetUsersByEmail(emails.ToArray());

                foreach (var email in emails)
                {
                    var userByEmail = users.FirstOrDefault(x => x.Email == email);

                    notifications.Add(new Notification(null, user, deal, email)
                    {
                        Action = NotificationAction.ParticipantAdd,
                        DealParticipant = userAdded.DisplayName ?? userAdded.EmailAddress,
                        DealRole = userAdded.Role ?? " participant ",
                        NewObject = userAdded.DisplayName ?? userAdded.EmailAddress,
                    });

                    if (userByEmail != null && (!await _userService.IsNotificationRegistered(userByEmail?.Id, NotificationAction.ParticipantAdd)))
                    {
                        emails.Remove(email);
                    }
                }

                await _emailService.ParticipantAddedNotification(user, deal, emails, userAdded);
                await CreateAll(notifications);
            }
        }

        public async Task ParticipantRemovedNotification(User user, DealModel deal, DealParticipant userRemoved)
        {
            var participants = await _dealParticipantService.GetParticipantsByDealId(deal.Id);
            List<string> emails = participants.Where(x => x.EmailAddress != user?.Email)
                .Select(x => x.EmailAddress)
                .ToList();

            if (emails != null && emails.Any())
            {
                var notifications = new List<Notification>();
                List<User> users = await _userService.BatchGetUsersByEmail(emails.ToArray());

                foreach (var email in emails)
                {
                    var userByEmail = users.FirstOrDefault(x => x.Email == email);

                    notifications.Add(new Notification(null, user, deal, email)
                    {
                        Action = NotificationAction.ParticipantRemove,
                        DealParticipant = userRemoved.DisplayName ?? userRemoved.EmailAddress?.ToString(),
                    });

                    if (!await _userService.IsNotificationRegistered(userByEmail?.Id, NotificationAction.ParticipantRemove))
                    {
                        emails.Remove(email);
                    }
                }
                await _emailService.ParticipantRemovedNotification(user, deal, emails, userRemoved);
                await CreateAll(notifications);
            }
        }

        public async Task ParticipantPermissionsModified(User user, DealModel deal, List<string> newPermissions, List<string> oldPermissions, DealParticipant dealParticipant)
        {
            var dealParticipants = await _dealParticipantService.GetParticipantsByDealId(deal.Id);
            var adminEmails = dealParticipants.Where(x => x.DealPermissions.Contains("Deal.Admin")).Select(x => x.EmailAddress).ToList();
            List<Notification> notifications = new List<Notification>();
            if (adminEmails.Any())
            {
                foreach (var email in adminEmails)
                {
                    foreach (var v in newPermissions)
                    {
                        if (v.StartsWith("Deal.") && v.Contains("Deal.Admin"))
                        {
                            notifications.Add(new Notification(null, user, deal, email)
                            {
                                Action = NotificationAction.ParticipantAddedAsAdmin,
                                DealParticipant = dealParticipant.DisplayName ?? dealParticipant.EmailAddress,
                            });

                            User userByEmail = await _userService.GetUserByEmail(email);
                            if (userByEmail != null && !await _userService.IsNotificationRegistered(userByEmail?.Id, NotificationAction.ParticipantAddedAsAdmin))
                            {
                                adminEmails.Remove(email);
                            }
                        }
                        else if (v.StartsWith("Deal.") && v.Contains("Deal.None") || v.Contains("Deal.Read") || v.Contains("Deal.Write"))
                        {
                            notifications.Add(new Notification(null, user, deal, email)
                            {
                                Action = NotificationAction.ParticipantPermissionUpdate,
                                DealParticipant = dealParticipant.DisplayName ?? dealParticipant.EmailAddress,
                                OldObject = oldPermissions.Where(x => x.StartsWith("Deal.")).FirstOrDefault() ?? "Deal Admin",
                                NewObject = v,
                                PropertyChanged = "Deal Permission",
                            });
                            User userByEmail = await _userService.GetUserByEmail(email);
                            if (userByEmail != null && !await _userService.IsNotificationRegistered(userByEmail?.Id, NotificationAction.ParticipantPermissionUpdate))
                            {
                                adminEmails.Remove(email);
                            }
                        }
                        else if (v.StartsWith("Expenditures."))
                        {
                            notifications.Add(new Notification(null, user, deal, email)
                            {
                                Action = NotificationAction.ParticipantPermissionUpdate,
                                DealParticipant = dealParticipant.DisplayName ?? dealParticipant.EmailAddress,
                                OldObject = oldPermissions.Where(x => x.StartsWith("Expenditures.")).FirstOrDefault() ?? "Deal Admin",
                                NewObject = v,
                                PropertyChanged = "Expenditures Permission",
                            });

                            User userByEmail = await _userService.GetUserByEmail(email);
                            if (userByEmail != null && !await _userService.IsNotificationRegistered(userByEmail?.Id, NotificationAction.ParticipantPermissionUpdate))
                            {
                                adminEmails.Remove(email);
                            }
                        }
                        else if (v.StartsWith("Performance."))
                        {
                            notifications.Add(new Notification(null, user, deal, email)
                            {
                                Action = NotificationAction.ParticipantPermissionUpdate,
                                DealParticipant = dealParticipant.DisplayName ?? dealParticipant.EmailAddress,
                                OldObject = oldPermissions.Where(x => x.StartsWith("Performance.")).FirstOrDefault() ?? "Deal Admin",
                                NewObject = v,
                                PropertyChanged = "Performance Permission",
                            });

                            User userByEmail = await _userService.GetUserByEmail(email);
                            if (userByEmail != null && !await _userService.IsNotificationRegistered(userByEmail?.Id, NotificationAction.ParticipantPermissionUpdate))
                            {
                                adminEmails.Remove(email);
                            }
                        }
                    }
                }
                await _emailService.ParticipantPermissionModified(user, deal, adminEmails?.Where(x => x != user.Email).ToList(), newPermissions, oldPermissions, dealParticipant);
            }
            await CreateAll(notifications);
        }

        public async Task ParticipantModified(User user, DealModel deal, List<DealParticipant> participants, Dictionary<string, List<ConcurrencyItem>> variances)
        {
            if (participants.Count == 0 || participants == null)
            {
                return;
            }

            List<string> dealParticipantEmails = participants.Where(x => x.DealPermissions.Contains("Deal.Admin")).Select(x => x.EmailAddress).ToList();
            if (!dealParticipantEmails.Any())
            {
                return;
            }

            List<Notification> notifications = new List<Notification>();
            List<string> emails = new List<string>();
            foreach (var participant in participants)
            {
                foreach (var v in variances)
                {
                    foreach (var variance in variances.Where(x => x.Key == v.Key).SelectMany(x => x.Value))
                    {
                        if (variance.Prop == "Role")
                        {
                            if (variance.FromDB?.ToString() == "")
                            {
                                variance.FromDB = "no role";
                            }
                            if (variance.Updated?.ToString() == "")
                            {
                                variance.Updated = "no role";
                            }
                            notifications.Add(new Notification(null, user, deal, participant.EmailAddress)
                            {
                                Action = NotificationAction.ParticipantRoleUpdated,
                                DealParticipant = participant.DisplayName ?? participant.EmailAddress,
                                OldRole = variance.FromDB?.ToString() ?? "no role",
                                DealRole = variance.Updated?.ToString() ?? "no role",
                                PropertyChanged = variance.Prop,
                            });
                        }
                        else if (variance.Prop == "IsPublic" && (bool)variance.FromDB == true)
                        {
                            notifications.Add(new Notification(null, user, deal, participant.EmailAddress)
                            {
                                Action = NotificationAction.ParticipantPublic,
                                DealParticipant = participant.DisplayName ?? participant.EmailAddress,
                                OldObject = variance.FromDB?.ToString(),
                                NewObject = variance.Updated?.ToString(),
                                PropertyChanged = variance.Prop,
                            });

                            User userByEmail = await _userService.GetUserByEmail(participant.EmailAddress);
                            if (userByEmail?.Confirmed == true && await _userService.IsNotificationRegistered(userByEmail?.Id, NotificationAction.ParticipantPublic))
                            {
                                emails.Add(participant.EmailAddress);
                            }
                        }
                        else if (variance.Prop == "IsPublic" && (bool)variance.FromDB == false)
                        {
                            notifications.Add(new Notification(null, user, deal, participant.EmailAddress)
                            {
                                Action = NotificationAction.ParticipantPrivate,
                                DealParticipant = participant.DisplayName ?? participant.EmailAddress,
                                OldObject = variance.FromDB?.ToString(),
                                NewObject = variance.Updated?.ToString(),
                                PropertyChanged = variance.Prop,
                            });

                            User userByEmail = await _userService.GetUserByEmail(participant.EmailAddress);
                            if (userByEmail?.Confirmed == true && await _userService.IsNotificationRegistered(userByEmail?.Id, NotificationAction.ParticipantPrivate))
                            {
                                emails.Add(participant.EmailAddress);
                            }
                        }
                        else
                        {
                            notifications.Add(new Notification(null, user, deal, participant.EmailAddress)
                            {
                                Action = NotificationAction.ParticipantModified,
                                DealParticipant = participant.DisplayName ?? participant.EmailAddress,
                                OldObject = variance.FromDB?.ToString() ?? "no input",
                                NewObject = variance.Updated?.ToString() ?? "no input",
                            });
                            User userByEmail = await _userService.GetUserByEmail(participant.EmailAddress);
                            if (userByEmail?.Confirmed == true && await _userService.IsNotificationRegistered(userByEmail?.Id, NotificationAction.ParticipantModified))
                            {
                                emails.Add(participant.EmailAddress);
                            }
                        }
                    }
                }
                if (emails.Any())
                {
                    await _emailService.ParticipantModified(user, deal, dealParticipantEmails, participants, variances);
                }
                await CreateAll(notifications);
            }
        }
        #endregion
        #region Expenditure Notifications
        public async Task ExpenditureAddedNotification(User user, DealModel deal, DealExpenditure newDealExpenditure)
        {
            List<string> emails = new List<string>();
            List<Notification> notifications = new List<Notification>();
            var participants = await _dealParticipantService.GetParticipantsByDealId(deal.Id);
            if (participants.Any())
            {
                foreach (var participant in participants)
                {
                    if (participant.DealPermissions.Contains("Deal.Admin") || participant.DealPermissions.Contains("Expenditures.Read") || participant.DealPermissions.Contains("Expenditures.Write"))
                    {
                        emails.Add(participant?.EmailAddress);
                    }
                }

                foreach (var email in emails)
                {
                    notifications.Add(new Notification(null, user, deal, email)
                    {
                        Action = NotificationAction.ExpenditureAdded,
                        ExpenditureValue = string.Format("{0:C}", newDealExpenditure.Value) ?? "0",
                        ExpenditureField = newDealExpenditure.DisplayName,
                    });
                    User userByEmail = await _userService.GetUserByEmail(email);
                    if (!await _userService.IsNotificationRegistered(userByEmail?.Id, NotificationAction.ExpenditureAdded))
                    {
                        emails.Remove(email);
                    }
                }

                await _emailService.ExpenditureAddedNotification(user, deal, emails, newDealExpenditure);
                await CreateAll(notifications);
            }
        }
        public async Task ExpendituresModifiedNotification(User user, DealModel deal, List<ConcurrencyItem> diffs, DealExpenditure dealExpenditure)
        {
            List<string?> emails = new List<string>();
            List<Notification> notifications = new List<Notification>();
            var participants = await _dealParticipantService.GetParticipantsByDealId(deal.Id);
            if (participants.Any())
            {
                foreach (var participant in participants)
                {
                    if (participant.DealPermissions.Contains("Deal.Admin") || participant.DealPermissions.Contains("Expenditures.Read") || participant.DealPermissions.Contains("Expenditures.Write"))
                    {
                        emails.Add(participant?.EmailAddress);
                    }
                }

                foreach (var email in emails)
                {
                    if (diffs.Count() == 1)
                    {
                        var diff = diffs.First();
                        if (diff?.FromDB?.ToString() == null || diff?.FromDB?.ToString() == "")
                        {
                            notifications.Add(new Notification(null, user, deal, email)
                            {
                                Action = NotificationAction.ExpenditureAdded,
                                ExpenditureValue = string.Format("{0:C}", dealExpenditure.Value) ?? "0",
                                ExpenditureField = dealExpenditure.DisplayName,
                            });
                            User userByEmail = await _userService.GetUserByEmail(email);
                            if (!await _userService.IsNotificationRegistered(userByEmail?.Id, NotificationAction.ExpenditureAdded))
                            {
                                emails.Remove(email);
                            }
                            await _emailService.ExpenditureAddedNotification(user, deal, emails, dealExpenditure);
                        }
                        else
                        {
                            notifications.Add(new Notification(null, user, deal, email)
                            {
                                Action = NotificationAction.ExpenditureChanged,
                                OldObject = string.Format("{0:C}", diff?.FromDB?.ToString()) ?? "0",
                                NewObject = string.Format("{0:C}", diff?.Updated?.ToString()) ?? "0",
                                PropertyChanged = dealExpenditure.DisplayName,
                            });
                            User userByEmail = await _userService.GetUserByEmail(email);
                            if (!await _userService.IsNotificationRegistered(userByEmail?.Id, NotificationAction.ExpenditureChanged))
                            {
                                emails.Remove(email);
                            }
                            await _emailService.ExpenditureModifiedNotification(user, deal, emails, dealExpenditure, diffs);
                        }
                    }
                    else if (diffs.Count() > 1)
                    {
                        foreach (var diff in diffs)
                        {
                            if (diff?.FromDB?.ToString() == null || diff?.FromDB?.ToString() == "")
                            {
                                //notifications created for audit purposes
                                notifications.Add(new Notification(null, user, deal, email)
                                {
                                    Action = NotificationAction.ExpenditureAddedAudit,
                                    ExpenditureValue = string.Format("{0:C}", dealExpenditure.Value) ?? "0",
                                    ExpenditureField = dealExpenditure.DisplayName,
                                });
                            }
                            else
                            {
                                notifications.Add(new Notification(null, user, deal, email)
                                {
                                    Action = NotificationAction.ExpenditureChangedAudit,
                                    OldObject = string.Format("{0:C}", diff?.FromDB?.ToString()) ?? "0",
                                    NewObject = string.Format("{0:C}", diff?.Updated?.ToString()) ?? "0",
                                    PropertyChanged = dealExpenditure.DisplayName,
                                });
                            }
                            StringBuilder bodyMessage = new StringBuilder();
                            bodyMessage.Append($" has changed the following Expenditures: ");
                            if (diff.EventType == ChangeEventType.Added)
                            {
                                bodyMessage.Append($" {diff.Prop} with a cost of {diff.Updated ?? "$0.00"} has been added; ");
                            }
                            else if (diff.EventType == ChangeEventType.Removed)
                            {
                                bodyMessage.Append($" {diff.Prop} has been removed; ");
                            }
                            else
                            {
                                bodyMessage.Append($" {diff.Prop} has been changed from {diff.FromDB?.ToString()} to {diff.Updated?.ToString()}; ");
                            }
                            notifications.Add(new Notification(null, user, deal, email)
                            {
                                Action = NotificationAction.ExpenditureMultipleChanges,
                                NewObject = bodyMessage?.ToString(),
                                DealState = deal.State?.ToString(),
                            });
                            User userByEmail = await _userService.GetUserByEmail(email);
                            if (!await _userService.IsNotificationRegistered(userByEmail?.Id, NotificationAction.ExpenditureChanged))
                            {
                                emails.Remove(email);
                            }
                            await _emailService.ExpenditureModifiedNotification(user, deal, emails, dealExpenditure, diffs);
                        }
                    }
                }
            }
            await CreateAll(notifications);
        }
        #endregion
        #region Performance Notifications
        public async Task PerformanceChangedNotification(User user, DealModel deal, List<ConcurrencyItem> variances)
        {
            var participants = await _dealParticipantService.GetParticipantsByDealId(deal.Id);
            var emails = new List<string>();
            foreach (var participant in participants)
            {
                if (participant.DealPermissions.IsOnlyDealAdmin() || participant.DealPermissions.Contains("Performance.Read") || participant.DealPermissions.Contains("Performance.Write"))
                {
                    emails.Add(participant.EmailAddress);
                }
            }

            List<Notification> notifications = new List<Notification>();
            foreach (var email in emails)
            {
                foreach (var variance in variances)
                {
                    string? propChanged = NotificationExtensions.GetPerformanceClassDisplayName(variance.Prop);
                    if (variance.Updated == null)
                    {
                        notifications.Add(new Notification(null, user, deal, email)
                        {
                            Action = NotificationAction.PerformanceAdded,
                            PropertyChanged = propChanged,
                            NewObject = variance.FromDB.ToString() ?? "0",
                        });
                        User userByEmail = await _userService.GetUserByEmail(email);
                        if (!await _userService.IsNotificationRegistered(userByEmail?.Id, NotificationAction.PerformanceAdded))
                        {
                            emails.Remove(email);
                        }
                    }
                    else
                    {
                        notifications.Add(new Notification(null, user, deal, email)
                        {
                            Action = NotificationAction.PerformanceUpdated,
                            PropertyChanged = propChanged,
                            OldObject = variance.Updated.ToString() ?? "0",
                            NewObject = variance.FromDB.ToString() ?? "0",
                        });
                        User userByEmail = await _userService.GetUserByEmail(email);
                        if (!await _userService.IsNotificationRegistered(userByEmail?.Id, NotificationAction.PerformanceUpdated))
                        {
                            emails.Remove(email);
                        }
                    }
                }
                await _emailService.PerformanceChangedNotification(user, deal, emails, variances);
            }
            await CreateAll(notifications);
        }

        public async Task PerformanceAccountChangedNotification(User user, DealModel deal, TopAccount topAccount, bool newAccount, List<ConcurrencyItem> variances)
        {
            var participants = await _dealParticipantService.GetParticipantsByDealId(deal.Id);
            if (participants.Any())
            {
                var emails = new List<string>();
                foreach (var participant in participants)
                {
                    if (participant.DealPermissions.IsOnlyDealAdmin() || participant.DealPermissions.Contains("Performance.Read") || participant.DealPermissions.Contains("Performance.Write"))
                    {
                        emails.Add(participant.EmailAddress);
                    }
                }
                if (emails.Any())
                {
                    foreach (var variance in variances)
                    {
                        List<Notification> notifications = new List<Notification>();
                        if (newAccount)
                        {
                            foreach (var email in emails)
                            {
                                notifications.Add(new Notification(null, user, deal, email)
                                {
                                    Action = NotificationAction.PerformanceTopAccountAdded,
                                    TopAccount = topAccount.AccountName ?? "No Account Name",
                                    TopAccountParAmount = string.Format("{0:C}", topAccount.ParAmount),
                                    TopAccountMaturityDateUTC = topAccount.MaturityDateUTC,
                                });
                                User userByEmail = await _userService.GetUserByEmail(email);
                                if (!await _userService.IsNotificationRegistered(userByEmail?.Id, NotificationAction.PerformanceTopAccountAdded))
                                {
                                    emails.Remove(email);
                                }
                                await _emailService.PerformanceTopAccountAddedNotification(user, deal, emails, topAccount);
                            }
                            return;
                        }
                        else if (variance.Prop.Contains("ParAmount"))
                        {
                            foreach (var email in emails)
                            {
                                notifications.Add(new Notification(null, user, deal, email)
                                {
                                    Action = NotificationAction.PerformanceTopAccountMarginAmountChanged,
                                    TopAccount = topAccount.AccountName ?? "No Account Name",
                                    TopAccountParAmount = string.Format("{0:C}", topAccount.ParAmount),
                                    TopAccountMaturityDateUTC = topAccount.MaturityDateUTC,
                                    OldObject = string.Format("{0:C}", variance.Updated.ToString()) ?? "no input",
                                    NewObject = string.Format("{0:C}", variance.FromDB.ToString()) ?? "no input",
                                });
                                User userByEmail = await _userService.GetUserByEmail(email);
                                if (!await _userService.IsNotificationRegistered(userByEmail?.Id, NotificationAction.PerformanceTopAccountMarginAmountChanged))
                                {
                                    emails.Remove(email);
                                }
                            }
                        }
                        else if (variance.Prop.Contains("AccountName"))
                        {
                            foreach (var email in emails)
                            {
                                notifications.Add(new Notification(null, user, deal, email)
                                {
                                    Action = NotificationAction.PerformanceTopAccountChanged,
                                    TopAccount = topAccount.AccountName ?? "No Account Name",
                                    TopAccountParAmount = string.Format("{0:C}", topAccount.ParAmount),
                                    TopAccountMaturityDateUTC = topAccount.MaturityDateUTC,
                                    OldObject = string.Format("{0:C}", variance.FromDB.ToString()) ?? "no input",
                                    NewObject = string.Format("{0:C}", variance.Updated.ToString()) ?? "no input",
                                    PropertyChanged = variance.Prop,
                                });
                                User userByEmail = await _userService.GetUserByEmail(email);
                                if (!await _userService.IsNotificationRegistered(userByEmail?.Id, NotificationAction.PerformanceTopAccountChanged))
                                {
                                    emails.Remove(email);
                                }
                            }
                        }
                        else if (variance.Prop.Contains("MaturityDate"))
                        {
                            foreach (var email in emails)
                            {
                                notifications.Add(new Notification(null, user, deal, email)
                                {
                                    Action = NotificationAction.PerformanceTopAccountMarginDateChanged,
                                    TopAccount = topAccount.AccountName ?? "No Account Name",
                                    TopAccountParAmount = string.Format("{0:C}", topAccount.ParAmount),
                                    TopAccountMaturityDateUTC = topAccount.MaturityDateUTC.Value.Date,
                                    OldObject = string.Format("{0:C}", variance.Updated.ToString()) ?? "no input",
                                    NewObject = string.Format("{0:C}", variance.FromDB.ToString()) ?? "no input",
                                });
                                User userByEmail = await _userService.GetUserByEmail(email);
                                if (!await _userService.IsNotificationRegistered(userByEmail?.Id, NotificationAction.PerformanceTopAccountMarginDateChanged))
                                {
                                    emails.Remove(email);
                                }
                            }
                        }
                        await _emailService.PerformanceTopAccountChangedNotification(user, deal, emails, topAccount, variances);
                        await CreateAll(notifications);
                    }
                }
            }
        }
        #endregion
        #region Firm Notifications
        public async Task FirmMemberAddedToFirmNotification(User user, Firm firm, FirmMember firmMember)
        {
            var emails = firm.FirmAdminEmails.Where(x => x != user.Email).Select(x => x).ToList();
            if (emails.Any())
            {


                List<Notification> notifications = new List<Notification>();

                foreach (var email in emails)
                {
                    notifications.Add(new Notification(null, user, null, email)
                    {
                        Action = NotificationAction.FirmMemberAddedToFirm,
                        FirmMember = firmMember.UserId ?? firmMember.EmailAddress,
                        FirmName = firm.Name,
                    });
                    User userByEmail = await _userService.GetUserByEmail(email);
                    if (userByEmail?.Confirmed != true || !await _userService.IsNotificationRegistered(userByEmail?.Id, NotificationAction.FirmMemberAddedToFirm))
                    {
                        emails.Remove(email);
                    }
                }
                await _emailService.FirmMemberAddedToFirm(user, firm, firmMember, emails);
                await CreateAll(notifications);
            }
        }
        #endregion
        #region Message Board Notification
        public async Task MessageBoardCommentNotification(User user, DealModel deal)
        {
            // Get participants
            List<DealParticipant> participants = (await _dealParticipantService.GetParticipantsByDealId(deal.Id)).Where(x => x.UserId != user.Id).ToList();
            List<Notification> notifications = new List<Notification>();
            // Emails to Notify
            List<string> emails = new List<string>();

            if (!participants.Any())
            {
                return;
            }

            foreach (var participant in participants)
            {
                notifications.Add(new Notification(null, user, deal, participant.EmailAddress)
                {
                    Action = NotificationAction.MessageBoardComment,
                });

                // Do we need to send email to user?
                if (await _userService.IsNotificationRegistered(participant.UserId, NotificationAction.MessageBoardComment))
                {
                    emails.Add(participant.EmailAddress);
                }
            }

            if (emails.Any())
            {
                await _emailService.MessageBoardCommentNotification(user, deal, emails);
            }
            await CreateAll(notifications);
        }
        #endregion
    }
}