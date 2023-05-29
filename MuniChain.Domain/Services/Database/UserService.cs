using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Shared.Helpers.Pagination;
using Shared.Models.Users;
using Data.DatabaseServices;
using Microsoft.AspNetCore.Components.Authorization;
using Shared.Models.Enums;

namespace Domain.Services.Database
{
    public interface IUserService
    {
        Task<bool> Create(User user);

        Task<List<User>> GetAllUsers();
        Task UpdateUser(User user);
        Task<List<User>> BatchGetUsersByEmail(string[] emails, int take = 250);
        Task<List<User>> BatchGetUsersById(string[] ids);
        Task<User> GetUserByEmail(string email);
        Task<User> GetUserById(string id);
        Task<int> CountFirmMembers(string firmId);
        Task<PaginatedResponse<User>> SearchPaged(PaginationDTO srchParams, bool IsConfirmed, Expression<Func<User, bool>> filter = null, bool showRole = true);
        Task<bool> IsNotificationRegistered(string? userID, NotificationAction action);
    }

    public class UserService : IUserService
    {
        private readonly IDbContextFactory<SqlDbContext> _factory;
        private readonly IFirmsService _firmsService;

        public UserService(IDbContextFactory<SqlDbContext> factory, IFirmsService firmsService)
        {
            _factory = factory;
            _firmsService = firmsService;
        }

        public async Task<bool> Create(User user)
        {
            using (var _dbContext = _factory.CreateDbContext())
            {
                _dbContext.Add(user);
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

        public async Task UpdateUser(User user)
        {
            using (var _dbContext = _factory.CreateDbContext())
            {
                _dbContext.Entry(user).State = EntityState.Modified;
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<User> GetUserById(string id)
        {
            using (var _dbContext = _factory.CreateDbContext())
            {
                return await _dbContext.Users.FindAsync(id);
            }
        }

        public async Task<List<User>> GetAllUsers()
        {
            using (var _dbContext = _factory.CreateDbContext())
            {
                return await _dbContext.Users.ToListAsync();
            }
        }

        public async Task<User> GetUserByEmail(string email)
        {
            using (var _dbContext = _factory.CreateDbContext())
            {
                return await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == email);
            }
        }

        public async Task<PaginatedResponse<User>> SearchPaged(PaginationDTO srchParams, bool IsConfirmed, Expression<Func<User, bool>> filter = null, bool showRole = true)
        {
            try
            {
                using (var _dbContext = _factory.CreateDbContext())
                {
                    PaginatedResponse<User> deals = new PaginatedResponse<User>();
                    //Search
                    IQueryable<User> queryable = _dbContext.Users.AsQueryable();

                    queryable = from a in queryable
                                select a;

                    if (filter != null)
                    {
                        queryable = queryable.Where(filter);
                    }

                    if (IsConfirmed)
                    {
                        queryable = queryable.Where(x => x.Confirmed == true);
                    }

                    if (!string.IsNullOrEmpty(srchParams.SearchText))
                    {
                        queryable = queryable.Search(srchParams.SearchColumns, srchParams.SearchText);

                    }

                    //Sort
                    queryable = queryable.OrderByDynamic(srchParams.SortField, srchParams.SortOrder);
                    //Pagination
                    await queryable.InsertPaginationParametersInResponse(srchParams.RecordsPerPage, deals);
                    var paginatedUsers = queryable.Paginate(srchParams.CurrentPage, srchParams.RecordsPerPage).ToList();

                    deals.Records = paginatedUsers;

                    // If we're not showing role in the table, we need to load the associated firms
                    if (!showRole)
                    {
                        var firmMemberInformation = await _firmsService.GetFirmsByUserEmail(deals.Records.Select(x => x.Email).ToArray());
                        if (firmMemberInformation?.Any() == true)
                        {
                            var firms = await _firmsService.GetByIds(firmMemberInformation.Select(x => x.FirmId).ToList());
                            if (firms?.Any() == true)
                            {
                                foreach (var user in deals.Records)
                                {
                                    var assocFirmId = firmMemberInformation?.FirstOrDefault(x => x.EmailAddress == user.Email)?.FirmId;
                                    if (assocFirmId != null)
                                    {
                                        user.AssociatedFirm = firms.FirstOrDefault(x => x.Id == assocFirmId);
                                    }
                                }
                            }

                        }

                    }

                    return deals;
                }
            }
            catch (Exception)
            {
                //Logger.LogError(ex.Message + "," + ex.StackTrace);
            }
            return null;
        }

        public async Task<List<User>> BatchGetUsersByEmail(string[] emails, int take = 250)
        {
            using (var _dbContext = _factory.CreateDbContext())
            {
                return await _dbContext.Users.Where(x => emails.Contains(x.Email)).Take(take).ToListAsync();
            }
        }

        public async Task<int> CountFirmMembers(string firmId)
        {
            using (var _dbContext = _factory.CreateDbContext())
            {
                return _dbContext.FirmMember.Count(x => x.FirmId == firmId);
            }
        }

        public async Task<List<User>> BatchGetUsersById(string[] ids)
        {
            using (var _dbContext = _factory.CreateDbContext())
            {
                return await _dbContext.Users.Where(x => ids.Contains(x.Id)).ToListAsync();
            }
        }
        public async Task<bool> IsNotificationRegistered(string? userID, NotificationAction action)
        {
            if (userID == null)
            {
                return false;
            }

            using (var _dbContext = _factory.CreateDbContext())
            {
                var pref = await _dbContext.UserNotificationPreference.Where(e => e.UserID == userID).FirstOrDefaultAsync();
                if (pref != null)
                {
                    switch (action)
                    {
                        case NotificationAction.DealArchived: return pref.Deal;
                        case NotificationAction.DealMadePublic: return pref.Deal;
                        case NotificationAction.DealUpdated: return pref.Deal;
                        case NotificationAction.DealUpdatedMultiple: return pref.Deal;
                        case NotificationAction.DealUpdatedAudit: return pref.Deal;
                        case NotificationAction.DealPartAdded: return pref.Deal;
                        case NotificationAction.DealPartRemoved: return pref.Deal;
                        case NotificationAction.ParticipantAdd: return pref.Participant;
                        case NotificationAction.ParticipantRemove: return pref.Participant;
                        case NotificationAction.ParticipantPermissionUpdate: return pref.Participant;
                        case NotificationAction.ParticipantAddedAsAdmin: return pref.Participant;
                        case NotificationAction.ParticipantRoleUpdated: return pref.Participant;
                        case NotificationAction.ParticipantPublic: return pref.Participant;
                        case NotificationAction.ParticipantPrivate: return pref.Participant;
                        case NotificationAction.ParticipantModified: return pref.Participant;
                        case NotificationAction.Saved: return pref.Saved;
                        case NotificationAction.Opened: return pref.Opened;
                        case NotificationAction.Sent: return pref.Sent;
                        case NotificationAction.Uploaded: return pref.Uploaded;
                        case NotificationAction.DocumentCommentedOn: return pref.DocumentCommented;
                        case NotificationAction.DocumentParticipantAdded: return pref.DocumentParticipant;
                        case NotificationAction.DocumentParticipantRemoved: return pref.DocumentParticipant;
                        case NotificationAction.DocumentVisibilityChangedToParticipants: return pref.Document;
                        case NotificationAction.DocumentVisibilityChangedToPublic: return pref.Document;
                        case NotificationAction.DocumentVisibilityChanged: return pref.Document;
                        case NotificationAction.DocumentAnnotationAdded: return pref.DocumentAnnotationAdded;
                        case NotificationAction.DocumentAnnotationChanged: return pref.DocumentAnnotationChanged;
                        case NotificationAction.DocumentAnnotationCommentAdded: return pref.DocumentAnnotationCommentAdded;
                        case NotificationAction.Signed: return pref.Signed;
                        case NotificationAction.Shared: return pref.Shared;
                        case NotificationAction.Deleted: return pref.Deleted;
                        case NotificationAction.Commented: return pref.Commented;
                        case NotificationAction.Completed: return pref.Completed;
                        case NotificationAction.Declined: return pref.Declined;
                        case NotificationAction.Revoked: return pref.Revoked;
                        case NotificationAction.Reassigned: return pref.Reassigned;
                        case NotificationAction.Expired: return pref.Expired;
                        case NotificationAction.ExpenditureAdded: return pref.Expenditure;
                        case NotificationAction.ExpenditureChanged: return pref.Expenditure;
                        case NotificationAction.PerformanceAdded: return pref.Performance;
                        case NotificationAction.PerformanceUpdated: return pref.Performance;
                        case NotificationAction.PerformanceTopAccountAdded: return pref.Performance;
                        case NotificationAction.PerformanceTopAccountChanged: return pref.Performance;
                        case NotificationAction.PerformanceTopAccountMarginAmountChanged: return pref.Performance;
                        case NotificationAction.PerformanceTopAccountMarginDateChanged: return pref.Performance;
                        case NotificationAction.FirmMemberAddedToFirm: return pref.Firm;
                        case NotificationAction.MessageBoardComment: return pref.MessageBoardComment;
                        default: return true;
                    }
                }
                return true;
            }
        }
    }
}
