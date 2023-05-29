using Data.DatabaseServices;
using Microsoft.EntityFrameworkCore;
using Shared.Models.DealComponents;
using Shared.Helpers.Pagination;
using Shared.Models.AppComponents;
using Shared.Models.Users;
using System.Linq.Expressions;

namespace Domain.Services.Database
{
    public interface IFirmsService
    {
        Task<bool> Create(Firm firm);
        Task<bool> Delete(string id);
        Task<PaginatedResponse<FirmMember>> SearchPaged(PaginationDTO srchParams, string firmId);
        Task<List<Firm>> GetByIds(List<string> ids);
        Task<Firm> GetByName(string name);
        Task<Firm> GetById(string id);
        Task<Firm> GetFirmWhereUserIsAdmin(string email);
        Task<Firm> GetFirmForUser(string email);
        Task<PaginatedResponse<Firm>> SearchPaged(PaginationDTO srchParams, bool isConfirmed, Expression<Func<Firm, bool>> filter = null, bool IncludeMembers = false);
        Task<List<Firm>> GetFirmsForDrpdown(bool isActive, FirmType? firmType = null, string? srchText = null, List<string>? selectedFirmIds = null);
        Task<bool> Update(Firm firm);
        Task<bool> UpdateFirmAdmins(string emails, string firmId);
        Task<bool> AddMembersToFirm(List<FirmMember> members);
        Task<bool> UpdateFirmMemberForNewUser(User user);
        Task<bool> AddMemberToFirm(User member, string firmId);
        Task<List<FirmMember>> GetFirmsByUserEmail(string[] emails);
        Task<bool> DeleteFirmMember(string userId);
    }

    public class FirmsService : IFirmsService
    {
        private readonly IDbContextFactory<SqlDbContext> _factory;

        public FirmsService(IDbContextFactory<SqlDbContext> factory)
        {
            _factory = factory;
        }

        public async Task<List<Firm>> GetFirmsForDrpdown(bool isActive, FirmType? firmType = null, string? srchText = null, List<string>? selectedFirmIds = null)
        {
            using (var _dbContext = _factory.CreateDbContext())
            {
                IQueryable<Firm> queryable = _dbContext.Firm.AsQueryable();
                if (firmType == null)
                {

                }
                else if (firmType == FirmType.Issuer)
                {
                    queryable = queryable.Where(x => x.FirmType == FirmType.Issuer);
                }
                else if (firmType == FirmType.Advisor)
                {
                    queryable = queryable.Where(x => x.FirmType == FirmType.Advisor);
                }
                else if (firmType == FirmType.BondCounsel)
                {
                    queryable = queryable.Where(x => x.FirmType == FirmType.BondCounsel);

                }

                if (isActive)
                {
                    queryable = queryable.Where(x => x.Confirmed == true);
                }

                if (!string.IsNullOrEmpty(srchText))
                {
                    queryable = queryable.Where(x => x.Name.Contains(srchText));
                }

                List<Firm> selectedFirms = new();
                if (selectedFirmIds?.Any() == true)
                {
                    selectedFirms = await GetByIds(selectedFirmIds);
                }

                var resp = await queryable.Take(50).ToListAsync();
                resp.AddRange(selectedFirms);
                return await queryable.ToListAsync();
            }
        }

        public async Task<Firm> GetFirmWhereUserIsAdmin(string email)
        {
            if (email == null)
            {
                return null;
            }

            using (var _dbContext = _factory.CreateDbContext())
            {
                var firmId = (await _dbContext.FirmMember.FirstOrDefaultAsync(x => x.EmailAddress == email && x.IsAdmin == true))?.FirmId;
                if (!string.IsNullOrEmpty(firmId))
                {
                    return await _dbContext.Firm.Include(x => x.Members).FirstOrDefaultAsync(x => x.Id == firmId);
                }
                else
                {
                    return null;
                }
            }
        }

        public async Task<Firm> GetFirmForUser(string email)
        {
            using (var _dbContext = _factory.CreateDbContext())
            {
                var firmId = (await _dbContext.FirmMember.FirstOrDefaultAsync(x => x.EmailAddress == email))?.FirmId;
                if (firmId != null)
                {
                    return await _dbContext.Firm.Include(x => x.Members).FirstOrDefaultAsync(x => x.Id == firmId);
                }
                else
                {
                    return null;
                }
            }
        }

        public async Task<List<FirmMember>> GetFirmsByUserEmail(string[] emails)
        {
            using (var _dbContext = _factory.CreateDbContext())
            {
                return _dbContext.FirmMember.Where(x => emails.Contains(x.EmailAddress)).ToList();
            }
        }

        public async Task<bool> UpdateFirmMemberForNewUser(User user)
        {
            using (var _dbContext = _factory.CreateDbContext())
            {
                var firmMember = await _dbContext.FirmMember.FirstOrDefaultAsync(x => x.EmailAddress == user.Email);

                if (firmMember == null)
                {
                    return false;
                }

                firmMember.UserId = user.Id;

                _dbContext.FirmMember.Update(firmMember);
                await _dbContext.SaveChangesAsync();
                return true;
            }
        }

        public async Task<PaginatedResponse<FirmMember>> SearchPaged(PaginationDTO srchParams, string firmId)
        {
            try
            {
                using (var _dbContext = _factory.CreateDbContext())
                {
                    PaginatedResponse<FirmMember> members = new PaginatedResponse<FirmMember>();
                    //Search
                    IQueryable<FirmMember> queryable = _dbContext.FirmMember.AsQueryable();

                    queryable = from a in queryable
                                select a;
                    queryable = queryable.Where(x => x.FirmId == firmId);


                    if (!string.IsNullOrEmpty(srchParams.SearchText))
                    {
                        queryable = queryable.Search(srchParams.SearchColumns, srchParams.SearchText);

                    }

                    //Sort
                    queryable = queryable.OrderByDynamic(srchParams.SortField, srchParams.SortOrder);
                    //Pagination
                    await queryable.InsertPaginationParametersInResponse(srchParams.RecordsPerPage, members);
                    var paginatedDeals = queryable.Paginate(srchParams.CurrentPage, srchParams.RecordsPerPage).ToList();

                    members.Records = paginatedDeals;
                    return members;
                }
            }
            catch (Exception)
            {
                //Logger.LogError(ex.Message + "," + ex.StackTrace);
            }
            return null;
        }

        public async Task<PaginatedResponse<Firm>> SearchPaged(PaginationDTO srchParams, bool isConfirmed, Expression<Func<Firm, bool>> filter = null, bool includeMembers = false)
        {
            try
            {
                using (var _dbContext = _factory.CreateDbContext())
                {
                    PaginatedResponse<Firm> firms = new PaginatedResponse<Firm>();
                    //Search
                    IQueryable<Firm> queryable = _dbContext.Firm.AsQueryable();

                    if (includeMembers)
                    {
                        queryable = queryable.Include(x => x.Members);
                    }

                    queryable = from a in queryable
                                select a;
                    if (isConfirmed)
                    {
                        queryable = queryable.Where(x => x.Confirmed == isConfirmed);
                    }
                    if (filter != null)
                    {
                        queryable = queryable.Where(filter);
                    }


                    if (!string.IsNullOrEmpty(srchParams.SearchText))
                    {
                        queryable = queryable.Search(srchParams.SearchColumns, srchParams.SearchText);

                    }

                    //Sort
                    queryable = queryable.OrderByDynamic(srchParams.SortField, srchParams.SortOrder);
                    //Pagination
                    await queryable.InsertPaginationParametersInResponse(srchParams.RecordsPerPage, firms);
                    var paginatedDeals = queryable.Paginate(srchParams.CurrentPage, srchParams.RecordsPerPage).ToList();

                    firms.Records = paginatedDeals;
                    return firms;
                }
            }
            catch (Exception)
            {
                //Logger.LogError(ex.Message + "," + ex.StackTrace);
            }
            return null;
        }


        public async Task<List<Firm>> GetByIds(List<string> ids)
        {
            if (ids == null || !ids.Any())
            {
                return new List<Firm>();
            }

            using (var _dbContext = _factory.CreateDbContext())
            {
                return await _dbContext.Firm.Where(x => ids.Contains(x.Id)).Include(x => x.Members).ToListAsync();
            }
        }

        public async Task<Firm> GetById(string id)
        {
            using (var _dbContext = _factory.CreateDbContext())
            {
                return await _dbContext.Firm.Include(x => x.Members).FirstOrDefaultAsync(x => x.Id == id);
            }
        }

        public async Task<Firm> GetByName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            using (var _dbContext = _factory.CreateDbContext())
            {
                return await _dbContext.Firm.FirstOrDefaultAsync(x => x.Name == name);
            }
        }

        public async Task<bool> Create(Firm firm)
        {
            using (var _dbContext = _factory.CreateDbContext())
            {
                firm.Id = Guid.NewGuid().ToString();
                _dbContext.Add(firm);
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

        public async Task<bool> AddMembersToFirm(List<FirmMember> members)
        {
            using (var _dbContext = _factory.CreateDbContext())
            {
                _dbContext.AddRange(members);
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
        public async Task<bool> AddMemberToFirm(User member, string firmId)
        {
            FirmMember memberToAdd = new FirmMember()
            {
                EmailAddress = member.Email,
                FirmId = firmId,
                Id = Guid.NewGuid().ToString(),
                IsAdmin = false,
                UserId = member.Id
            };

            using (var _dbContext = _factory.CreateDbContext())
            {
                var exists = await _dbContext.FirmMember.FirstOrDefaultAsync(f => f.EmailAddress == member.Email);
                if (exists != null)
                {
                    _dbContext.Remove(exists);
                }
                _dbContext.Add(memberToAdd);
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

        public async Task<bool> Update(Firm firm)
        {
            using (var _dbContext = _factory.CreateDbContext())
            {
                var foundFirm = await GetById(firm.Id);
                if (foundFirm != null)
                {
                    _dbContext.Entry(firm).State = EntityState.Modified;
                    await _dbContext.SaveChangesAsync();
                    return true;
                }
                else
                {
                    return await Create(firm);
                }
            }
        }

        public async Task<bool> UpdateFirmAdmins(string emailList, string firmId)
        {
            var emails = emailList.Split(",");
            using (var _dbContext = _factory.CreateDbContext())
            {
                var currentAdmins = _dbContext.FirmMember.Where(x => x.IsAdmin && x.FirmId == firmId);
                var currentMembersOfFirm = _dbContext.FirmMember.Where(x => emailList.Contains(x.EmailAddress));

                _dbContext.RemoveRange(currentAdmins);
                _dbContext.RemoveRange(currentMembersOfFirm);

                // Current firm associations

                List<FirmMember> newFirmAdmins = new();

                foreach (var admin in emails)
                {
                    newFirmAdmins.Add(new FirmMember()
                    {
                        EmailAddress = admin,
                        Id = Guid.NewGuid().ToString(),
                        IsAdmin = true,
                        FirmId = firmId
                    });
                }

                if (newFirmAdmins.Count() > 0)
                {
                    _dbContext.AddRange(newFirmAdmins);
                }
                await _dbContext.SaveChangesAsync();

                return true;
            }
        }

        public async Task<bool> Delete(string id)
        {
            using (var _dbContext = _factory.CreateDbContext())
            {
                var firm = await _dbContext.Firm.FindAsync(id);
                if (firm == null)
                {
                    return false;
                }

                _dbContext.Firm.Remove(firm);
                await _dbContext.SaveChangesAsync();
                return true;
            }
        }

        public async Task<bool> DeleteFirmMember(string email)
        {
            using (var _dbContext = _factory.CreateDbContext())
            {
                var member = await _dbContext.FirmMember.FirstOrDefaultAsync(x => x.EmailAddress == email);
                if (member == null)
                {
                    return false;
                }

                _dbContext.FirmMember.Remove(member);
                await _dbContext.SaveChangesAsync();
                return true;
            }
        }
    }
}