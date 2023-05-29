using Data.DatabaseServices;
using Microsoft.EntityFrameworkCore;
using Shared.Helpers;
using Shared.Helpers.Pagination;
using Shared.Models.AppComponents;
using Shared.Models.DealComponents;
using Shared.Models.Enums;
using Shared.Models.Users;
using Syncfusion.Blazor.Data;

namespace Domain.Services.Database
{
    public interface IDealService
    {
        Task Create(DealModel deal);
        Task<DealModel?> GetById(string id, DealViewType? viewType);
        Task<List<DealModel>> GetByIds(List<string> ids);
        Task<Tuple<DealModel?, bool>> Update(User currentUser, DealModel deal, bool isMergeSubmit);
        Task<List<DealModel>> GetAuditDeals(string dealId, bool GetAllDeals = false);
        Task UpdatePerformance(Performance performance);
        Task ChangeStatus(User currentUser, DealModel deal);
        Task UnArchiveAllDeals(User user, DealModel deal);
        Task LockUnlockDeal(DealModel deal);
        Task<Tuple<int, decimal?>> GetDealsSumByFirmID(string firmID, bool isPublished);
        Task<PaginatedResponse<DealModel>> SearchPaged(PaginationDTO srchParams, bool isPublic, string userId, string firmID);
        Task CreateUpdateDealExpenditure(List<DealExpenditure> lstExp);
        Task<List<DealExpenditure>?> GetDealExpenditureByDealID(string dealID);
        Task<bool> HasBeenPublished(string id, string historyDealId);
        Task<bool> IsLatestPublished(string dealId);
        Task<Performance> GetPerformanceByDealId(string dealID);
    }

    public class DealService : IDealService
    {
        private readonly IDbContextFactory<SqlDbContext> _factory;
        private readonly IDealParticipantService _dealParticipantService;

        public DealService(IDbContextFactory<SqlDbContext> factory, IDealParticipantService dealParticipantService)
        {
            _dealParticipantService = dealParticipantService;
            _factory = factory;
        }

        public async Task<DealModel?> GetById(string id, DealViewType? viewType)
        {
            using (var _dbContext = _factory.CreateDbContext())
            {
                DealModel deal = null;

                if (viewType == DealViewType.NotFound)
                {
                    return null;
                }
                if (viewType == DealViewType.ByID)
                {
                    deal = await _dbContext.Deals.Include(x => x.Series)
                                                .ThenInclude(x => x.Maturities)
                                                .FirstOrDefaultAsync(x => x.Id == id);
                }

                var originalDealId = (await _dbContext.Deals.FindAsync(id))?.HistoryDealID;

                if (viewType == DealViewType.LatestPublished)
                {

                    deal = await _dbContext.Deals.Where(
                            x => x.Status == "Published"
                            && x.HistoryDealID == id || x.Id == id
                                                    ).Include(x => x.Series)
                                                    .ThenInclude(x => x.Maturities)
                                                    .OrderByDescending(x => x.CreatedDateUTC)
                                                    .FirstOrDefaultAsync();
                }
                else if (viewType == DealViewType.Latest)
                {
                    if (originalDealId != null)
                    {
                        deal = await _dbContext.Deals.Where(x => x.Id == originalDealId).Include(x => x.Series)
                                                .ThenInclude(x => x.Maturities)
                                                .OrderByDescending(x => x.CreatedDateUTC)
                                                .FirstOrDefaultAsync();
                    }
                    // If not historical deal
                    else
                    {
                        deal = await GetById(id, DealViewType.ByID);
                    }
                }
                else if (viewType == DealViewType.DealReadFalse)
                {
                    // Return minimal deal for dealreadfalse
                    deal = await _dbContext.Deals.Select(p => new DealModel() { Id = p.Id, Issuer = p.Issuer, HistoryDealID = p.HistoryDealID, CreatedBy = p.CreatedBy, RowVersion = p.RowVersion, Status = p.Status, IsLocked = p.IsLocked, Size = p.Size, State = p.State }).FirstOrDefaultAsync(x => x.Id == id);
                }

                return deal;
            }
        }

        public async Task<List<DealModel>> GetByIds(List<string> ids)
        {
            using (var _dbContext = _factory.CreateDbContext())
            {
                return await _dbContext.Deals.Where(x => string.IsNullOrEmpty(x.HistoryDealID)).Include(x => x.Series)
                                            .ThenInclude(x => x.Maturities)
                                            .Include(x => x.Performance)
                                            .ThenInclude(x => x.TopAccountList)
                                            .Where(x => ids.Contains(x.Id))
                                            .Take(250)
                                            .ToListAsync();
            }
        }

        public async Task<List<DealModel>> GetAuditDeals(string dealId, bool GetAllDeals = false)
        {
            using (var _dbContext = _factory.CreateDbContext())
            {
                if (GetAllDeals)
                {
                    return await _dbContext.Deals.Where(x => x.HistoryDealID == dealId).Include(x => x.Series)
                                                                .ThenInclude(x => x.Maturities)
                                                                .Include(x => x.Performance)
                                                                .ThenInclude(x => x.TopAccountList)
                                                                .ToListAsync();
                }

                return await _dbContext.Deals.Where(x => x.HistoryDealID == dealId && x.Status == "Published").Include(x => x.Series)
                                            .ThenInclude(x => x.Maturities)
                                            .Include(x => x.Performance)
                                            .ThenInclude(x => x.TopAccountList)
                                            .ToListAsync();
            }
        }

        public async Task<bool> HasBeenPublished(string id, string historyDealId)
        {
            using (var _dbContext = _factory.CreateDbContext())
            {
                // If its the first copy of the deal
                if (historyDealId == null)
                {
                    return await _dbContext.Deals.Where(x => x.HistoryDealID == id && x.Status == "Published").AnyAsync();
                }
                else
                {
                    return await _dbContext.Deals.Where(x => x.HistoryDealID == historyDealId && x.Status == "Published").AnyAsync();
                }
            }
        }

        public async Task<bool> IsLatestPublished(string dealId)
        {
            using (var _dbContext = _factory.CreateDbContext())
            {
                var deal = await _dbContext.Deals.FindAsync(dealId);

                if (deal != null)
                {
                    var isHistoryDeal = deal.HistoryDealID != null;
                    DealModel latestPublishedDeal = new();
                    if (isHistoryDeal)
                    {
                        latestPublishedDeal = await _dbContext.Deals.Where(x => x.HistoryDealID == deal.HistoryDealID && x.Status == "Published")
                                                                        .OrderByDescending(x => x.CreatedDateUTC)
                                                                        .FirstOrDefaultAsync();
                    }

                    return latestPublishedDeal?.Id == dealId;
                }
                else
                {
                    return false;
                }
            }
        }

        public async Task Create(DealModel deal)
        {
            using (var _dbContext = _factory.CreateDbContext())
            {
                deal.CreatedDateUTC = DateTime.Now.ToUniversalTime();
                deal.LastModifiedDateTimeUTC = DateTime.Now.ToUniversalTime();
                _dbContext.Deals.Add(deal);
                await _dbContext.SaveChangesAsync();
            }
        }
        public async Task UpdatePerformance(Performance performance)
        {
            using (var _dbContext = _factory.CreateDbContext())
            {
                List<TopAccount> topAccounts = new List<TopAccount>();
                var dealFromDb = await _dbContext.Deals.Where(x => string.IsNullOrEmpty(x.HistoryDealID)).Include(x => x.Performance).ThenInclude(x => x.TopAccountList).Include(x => x.Series).ThenInclude(x => x.Maturities).AsNoTracking().FirstOrDefaultAsync(x => x.Id == performance.DealModelId);
                if (dealFromDb != null && dealFromDb.Performance != null)
                {
                    if (!dealFromDb.Performance.RowVersion.SequenceEqual(performance.RowVersion))
                    {
                        throw new DbUpdateConcurrencyException();
                    }
                }
                var topAccountFromDb = dealFromDb.Performance.TopAccountList;

                // Remove all series
                _dbContext.RemoveRange(topAccountFromDb);
                await _dbContext.SaveChangesAsync();

                foreach (var item in performance.TopAccountList.ToList())
                {
                    item.PerformanceId = performance.Id;
                    if (topAccountFromDb.Contains(item))
                    {
                        _dbContext.Entry(topAccountFromDb.Single(x => x.Id == item.Id)).CurrentValues.SetValues(item);
                    }
                    else
                    {
                        _dbContext.Add(item);
                    }
                }

                _dbContext.Performance.Update(performance);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<Tuple<DealModel?, bool>> Update(User currentUser, DealModel deal, bool isMergeSubmit)
        {
            using (var _dbContext = _factory.CreateDbContext())
            {
                //Update properties first so that when cloned history maintained
                deal.Status = "Draft";
                deal.LastModifiedBy = currentUser.Id;
                deal.LastModifiedByDisplayName = currentUser.DisplayName;
                deal.LastModifiedDateTimeUTC = DateTime.Now.ToUniversalTime();

                List<Maturity> maturities = new List<Maturity>();
                var dealFromDb = _dbContext.Deals.Where(x => string.IsNullOrEmpty(x.HistoryDealID)).Include(x => x.Series).ThenInclude(x => x.Maturities).FirstOrDefault(x => x.Id == deal.Id);

                if (!isMergeSubmit)
                {
                    if (dealFromDb is not null && !dealFromDb.RowVersion.SequenceEqual(deal.RowVersion))
                    {
                        return Tuple.Create(dealFromDb, true);
                        //throw new DbUpdateConcurrencyException();
                    }
                }

                // Insert Audit Trail before update
                DealModel dealUpdated = deal.Clone();
                dealUpdated.HistoryDealID = deal.Id; //First assign HistoryDealID before creating new id
                dealUpdated.Id = Guid.NewGuid().ToString();
                dealUpdated.CreatedDateUTC = DateTime.Now.ToUniversalTime();
                dealUpdated.LastModifiedBy = currentUser.Id;
                dealUpdated.LastModifiedByDisplayName = currentUser.DisplayName;
                dealUpdated.LastModifiedDateTimeUTC = DateTime.Now.ToUniversalTime();
                
                //Performance 
                if (deal.Performance is not null)
                {
                    dealUpdated.Performance.HistoryPerformanceID = deal.Performance.Id;//First assign HistoryPerformanceID before creating new id
                    dealUpdated.Performance.Id = Guid.NewGuid().ToString();
                    dealUpdated.Performance.CreatedDateUTC = DateTime.Now.ToUniversalTime();
                    dealUpdated.Performance.DealModelId = dealUpdated.Id;

                    if (deal?.Performance?.TopAccountList?.Any() == true)
                    {
                        foreach (var item in deal?.Performance?.TopAccountList)
                        {
                            item.HistoryTopAccountID = item.Id;//First assign HistoryTopAccountID before creating new id
                            item.Id = Guid.NewGuid().ToString();
                            item.CreatedDateUTC = DateTime.Now.ToUniversalTime();
                        }
                    }
                }

                //Series
                foreach (var item in dealUpdated?.Series?.OrderBy(x => x.CreatedDateUTC))
                {
                    //_dbContext.Entry(item).Property(x => x.IsPublished).IsModified = true;
                    item.HistorySeriesID = item.Id;//First assign HistorySeriesID before creating new id
                    item.Id = Guid.NewGuid().ToString();
                    // CreatedTime is set when series is created in UI
                    item.CreatedDateUTC = DateTime.Now.ToUniversalTime();
                    item.DealModelId = dealUpdated.Id;
                    //Maturities
                    foreach (var itemMat in item?.Maturities)
                    {
                        //_dbContext.Entry(itemMat).Property(x => x.IsPublished).IsModified = true;
                        itemMat.GlobalMaturityID = itemMat.GlobalMaturityID;//First assign HistorySeriesID before creating new id
                        itemMat.HistoryMaturityID = itemMat.Id;//First assign HistorySeriesID before creating new id
                        itemMat.Id = Guid.NewGuid().ToString();
                        itemMat.CreatedDateUTC = DateTime.Now.ToUniversalTime();
                        itemMat.SeriesId = item.Id;
                    }
                }


                var seriesFromDb = dealFromDb.Series;
                dealFromDb.Series.ForEach(x => x.Maturities.ForEach(x => maturities.Add(x)));

                // Remove all series
                _dbContext.RemoveRange(seriesFromDb);
                _dbContext.RemoveRange(maturities);
                await _dbContext.SaveChangesAsync();

                foreach (var item in deal.Series.ToList())
                {
                    item.DealModelId = deal.Id;
                    if (seriesFromDb.Contains(item))
                    {
                        _dbContext.Entry(seriesFromDb.Single(x => x.Id == item.Id)).CurrentValues.SetValues(item);
                    }
                    else
                    {
                        _dbContext.Add(item);
                    }
                }

                _dbContext.Entry(dealFromDb).CurrentValues.SetValues(deal);

                // Insert Audit

                await _dbContext.Deals.AddAsync(dealUpdated);
                await _dbContext.SaveChangesAsync();

                return Tuple.Create(dealFromDb, false);
            }
        }

        public async Task ChangeStatus(User currentUser, DealModel deal)
        {
            using (var _dbContext = _factory.CreateDbContext())
            {
                #region Deal
                if (deal.Status == "Archived")
                {
                    await ArchiveAllDeals(currentUser, deal);
                    return;
                }

                //Update properties first so that when cloned history maintained
                deal.LastModifiedBy = currentUser.Id;
                deal.LastModifiedByDisplayName = currentUser.DisplayName;
                deal.LastModifiedDateTimeUTC = DateTime.Now.ToUniversalTime();

                var dealFromDb = await _dbContext.Deals.Where(x => string.IsNullOrEmpty(x.HistoryDealID)).AsNoTracking().FirstOrDefaultAsync(x => x.Id == deal.Id);
                if (dealFromDb != null)
                {
                    if (!dealFromDb.RowVersion.SequenceEqual(deal.RowVersion))
                    {
                        throw new DbUpdateConcurrencyException();
                    }
                }

                if (deal.Status == "Published")
                {
                    DealModel dealPublished = deal.Clone();
                    deal.Status = "Draft";

                    dealPublished.HistoryDealID = deal.Id;//First assign HistoryDealID before creating new id
                    dealPublished.Id = Guid.NewGuid().ToString();
                    dealPublished.CreatedDateUTC = DateTime.Now.ToUniversalTime();
                    dealPublished.LastModifiedDateTimeUTC = DateTime.Now.ToUniversalTime();

                    if (deal.Performance is not null)
                    {
                        dealPublished.Performance.HistoryPerformanceID = deal.Performance.Id;//First assign HistoryPerformanceID before creating new id
                        dealPublished.Performance.Id = Guid.NewGuid().ToString();
                        dealPublished.Performance.CreatedDateUTC = DateTime.Now.ToUniversalTime();
                        dealPublished.Performance.DealModelId = dealPublished.Id;
                    }
                    if (dealPublished.Performance is not null)
                    {
                        foreach (var item in dealPublished?.Performance?.TopAccountList)
                        {
                            item.HistoryTopAccountID = item.Id;//First assign HistoryTopAccountID before creating new id
                            item.Id = Guid.NewGuid().ToString();
                            item.CreatedDateUTC = DateTime.Now.ToUniversalTime();
                        }
                    }

                    var latestPublishedDeal = await GetById(deal.Id, DealViewType.LatestPublished);

                    //Build series of Published deal
                    dealPublished.Series = new List<Series>();
                    foreach (var item in deal.Series.OrderBy(x => x.CreatedDateUTC))
                    {
                        Series latestPublishedSeries = latestPublishedDeal?.Series?.FirstOrDefault(x => x.GlobalSeriesID == item.GlobalSeriesID);
                        Series aboutToBePublishedSeries = deal?.Series?.ElementAt(deal.Series.IndexOf(item));

                        // If you choose not to publish the series
                        if (!item.IsPublished)
                        {
                            // if deal has been published and there are series
                            if (latestPublishedDeal != null && latestPublishedDeal.Series.Any())
                            {
                                // Find matching series
                                if (latestPublishedSeries != null)
                                {
                                    aboutToBePublishedSeries = latestPublishedSeries.Clone();
                                    aboutToBePublishedSeries.GlobalSeriesID = Guid.NewGuid().ToString();
                                    aboutToBePublishedSeries.Id = Guid.NewGuid().ToString();
                                    aboutToBePublishedSeries.CreatedDateUTC = DateTime.Now.ToUniversalTime();
                                }
                            }
                            else
                            {
                                aboutToBePublishedSeries = null;
                            }
                        }
                        else
                        {
                            aboutToBePublishedSeries = item.Clone();
                            aboutToBePublishedSeries.GlobalSeriesID = item.GlobalSeriesID;//First assign HistorySeriesID before creating new id
                            aboutToBePublishedSeries.HistorySeriesID = item.Id;//First assign HistorySeriesID before creating new id
                            aboutToBePublishedSeries.Id = Guid.NewGuid().ToString();
                            aboutToBePublishedSeries.CreatedDateUTC = DateTime.Now.ToUniversalTime();
                            aboutToBePublishedSeries.DealModelId = dealPublished.Id;
                        }

                        //Build series of Published deal
                        aboutToBePublishedSeries.Maturities = new List<Maturity>();
                        //Build Maturities of Published deal
                        foreach (var itemMat in item?.Maturities)
                        {
                            Maturity latestPublishedMaturity = latestPublishedSeries?.Maturities?.FirstOrDefault(x => x.GlobalMaturityID == itemMat.GlobalMaturityID);
                            Maturity aboutToBePublishedMaturity = item?.Maturities?.ElementAt(item.Maturities.IndexOf(itemMat));

                            if (!item?.IsPublishedMaturities == true)
                            {
                                // if deal has been published and there are series with maturities
                                if (latestPublishedSeries != null && latestPublishedSeries.Maturities.Any())
                                {
                                    // Find matching maturity
                                    if (latestPublishedMaturity != null)
                                    {
                                        aboutToBePublishedMaturity = latestPublishedMaturity.Clone();
                                        aboutToBePublishedMaturity.GlobalMaturityID = Guid.NewGuid().ToString();
                                        aboutToBePublishedMaturity.Id = Guid.NewGuid().ToString();
                                        aboutToBePublishedMaturity.CreatedDateUTC = DateTime.Now.ToUniversalTime();
                                        aboutToBePublishedSeries?.Maturities?.Add(aboutToBePublishedMaturity);
                                    }
                                }
                                else
                                {
                                    // If you dont want to publish maturities for a series
                                    // And there is no published copy already
                                    // dont insert history
                                    aboutToBePublishedMaturity = null;
                                }

                            }
                            else
                            {
                                aboutToBePublishedMaturity = itemMat.Clone();
                                aboutToBePublishedMaturity.GlobalMaturityID = itemMat.GlobalMaturityID;//First assign HistorySeriesID before creating new id
                                aboutToBePublishedMaturity.HistoryMaturityID = itemMat.Id;//First assign HistorySeriesID before creating new id
                                aboutToBePublishedMaturity.Id = Guid.NewGuid().ToString();
                                aboutToBePublishedMaturity.CreatedDateUTC = DateTime.Now.ToUniversalTime();
                                aboutToBePublishedMaturity.SeriesId = aboutToBePublishedSeries?.Id;
                            }
                            if (aboutToBePublishedMaturity != null)
                            {
                                aboutToBePublishedSeries?.Maturities.Add(aboutToBePublishedMaturity);
                            }
                        }
                        if (aboutToBePublishedSeries != null)
                            dealPublished.Series.Add(aboutToBePublishedSeries);
                    }
                    await _dbContext.Deals.AddAsync(dealPublished);
                }
                else
                {
                    _dbContext.Deals.Attach(deal);
                    _dbContext.Entry(deal).Property(x => x.Status).IsModified = true;
                }

                //Series
                foreach (var item in deal.Series)
                {
                    _dbContext.Entry(item).Property(x => x.IsPublished).IsModified = true;
                    _dbContext.Entry(item).Property(x => x.IsPublishedMaturities).IsModified = true;
                }
                #endregion

                await _dbContext.SaveChangesAsync();
            }
        }

        private async Task ArchiveAllDeals(User user, DealModel deal)
        {
            List<DealModel> allDeals = new();
            using (var _dbContext = _factory.CreateDbContext())
            {
                deal.OldStatus = "Draft";
                allDeals = await GetAuditDeals(deal.Id, true);

                // If archiving master deal
                if (deal.HistoryDealID == null)
                {
                    allDeals.ForEach(x => x.OldStatus = x.Status);
                    allDeals.ForEach(x => x.Status = "Archived");
                    deal.LastModifiedBy = user.Id;
                    deal.LastModifiedByDisplayName = user.DisplayName;
                    deal.LastModifiedDateTimeUTC = DateTime.UtcNow;

                    _dbContext.Deals.UpdateRange(allDeals);
                    _dbContext.Deals.Update(deal);
                    await _dbContext.SaveChangesAsync();
                }
                else
                {
                    throw new Exception("Trying to archive historical deal");
                }

            }
        }

        public async Task UnArchiveAllDeals(User user, DealModel deal)
        {
            List<DealModel> allDeals = new();
            using (var _dbContext = _factory.CreateDbContext())
            {
                allDeals = await GetAuditDeals(deal.Id, true);

                // If archiving master deal
                if (deal.HistoryDealID == null)
                {
                    allDeals.ForEach(x => x.Status = x.OldStatus);
                    deal.Status = "Draft";
                    deal.LastModifiedBy = user.Id;
                    deal.LastModifiedByDisplayName = user.DisplayName;
                    deal.LastModifiedDateTimeUTC = DateTime.UtcNow;

                    _dbContext.Deals.UpdateRange(allDeals);
                    _dbContext.Deals.Update(deal);
                    await _dbContext.SaveChangesAsync();
                }
                else
                {
                    throw new Exception("Trying to unarchive historical deal");
                }

            }
        }

        public async Task LockUnlockDeal(DealModel deal)
        {
            using (var _dbContext = _factory.CreateDbContext())
            {
                var dealFromDb = await _dbContext.Deals.Where(x => string.IsNullOrEmpty(x.HistoryDealID)).AsNoTracking().FirstOrDefaultAsync(x => x.Id == deal.Id);
                if (dealFromDb != null)
                {
                    if (!dealFromDb.RowVersion.SequenceEqual(deal.RowVersion))
                    {
                        throw new DbUpdateConcurrencyException();
                    }
                }
                _dbContext.Deals.Attach(deal);
                _dbContext.Entry(deal).Property(x => x.IsLocked).IsModified = true;
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<Tuple<int, decimal?>> GetDealsSumByFirmID(string firmId, bool isPublished)
        {

            using (var _dbContext = _factory.CreateDbContext())
            {
                // Get all members of firm
                List<FirmMember> firmMemberIds = _dbContext.FirmMember.Where(x => x.FirmId == firmId).ToList();
                var memberIds = firmMemberIds.Select(x => x.EmailAddress);

                IQueryable<DealModel> queryable;
                // Get deals by participant firm 
                if (isPublished)
                {
                    //Show the published latest history version
                    var latestDealIds = from a in _dbContext.DealParticipants.AsQueryable()
                                        where memberIds.Contains(a.EmailAddress)
                                        select a.DealId;

                    queryable = from a in _dbContext.Deals
                                join b in latestDealIds on a.HistoryDealID equals b
                                select a;

                    queryable = queryable.Where(x => x.Status == "Published").GroupBy(deal => deal.HistoryDealID)
                          .Select(group =>
                                new
                                {
                                    Name = group.Key,
                                    Deals = group.OrderByDescending(x => x.CreatedDateUTC).ToList()
                                })
                          .Select(group => group.Deals.FirstOrDefault()).AsQueryable();
                }
                else
                {
                    //Show the published latest history version
                    var qryPublishedLatest = from a in _dbContext.DealParticipants.AsQueryable()
                                             where memberIds.Contains(a.EmailAddress)
                                             select a.DealId;

                    queryable = from a in _dbContext.Deals.Where(x => string.IsNullOrEmpty(x.HistoryDealID))
                                join b in qryPublishedLatest on a.Id equals b
                                select a;
                }

                var deals = queryable?.ToList();
                int dealCount = deals.Count();
                decimal? dealsSum = deals.Sum(e => e.Size);
                return new Tuple<int, decimal?>(dealCount, dealsSum);
            }
        }
        public async Task<PaginatedResponse<DealModel>> SearchPaged(PaginationDTO srchParams, bool isPublic, string userId, string firmID)
        {
            try
            {
                using (var _dbContext = _factory.CreateDbContext())
                {
                    PaginatedResponse<DealModel> deals = new PaginatedResponse<DealModel>();
                    //Search
                    IQueryable<DealModel> queryable = _dbContext.Deals.AsQueryable();
                    queryable = queryable.Where(x => x.Status != "Archived");

                    if (isPublic)
                    {
                        //Show the published latest history version
                        var qryPublishedLatest = from a in queryable
                                                 where a.Status == "Published" && !string.IsNullOrEmpty(a.HistoryDealID)
                                                 group a by a.HistoryDealID into g
                                                 select new
                                                 {
                                                     HistoryDealID = g.Key,
                                                     MaxRowVersion = g.Max(i => i.RowVersion)
                                                 };
                        queryable = from a in queryable
                                    join b in qryPublishedLatest on a.RowVersion equals b.MaxRowVersion
                                    select a;
                    }
                    else
                    {
                        queryable = queryable.Where(x => string.IsNullOrEmpty(x.HistoryDealID));
                    }

                    if (!string.IsNullOrEmpty(firmID))
                    {
                        // Get all members of firm
                        List<string> firmMemberIds = _dbContext.FirmMember.Where(x => x.FirmId == firmID).Select(x => x.EmailAddress).ToList();

                        // Get deals by participant firm 
                        if (isPublic)
                        {
                            var dealIds = _dbContext.DealParticipants.Where(x => firmMemberIds.Contains(x.EmailAddress)).Select(x => x.DealId).Distinct();
                            queryable = from a in queryable
                                        join b in dealIds on a.HistoryDealID equals b
                                        select a;
                        }
                        else
                        {
                            var dealIds = _dbContext.DealParticipants.Where(x => firmMemberIds.Contains(x.EmailAddress)).Select(x => x.DealId).Distinct();

                            queryable = from a in queryable
                                        join b in dealIds on a.Id equals b
                                        select a;
                        }

                    }

                    if (!string.IsNullOrEmpty(userId))
                    {
                        var dealIds = await _dealParticipantService.GetDealsByUser(userId);

                        if (isPublic)
                        {
                            queryable = queryable.Where(x => dealIds.Contains(x.HistoryDealID));
                        }
                        else
                        {
                            queryable = queryable.Where(x => dealIds.Contains(x.Id));
                        }
                    }

                    if (!string.IsNullOrEmpty(srchParams.SearchText))
                    {
                        queryable = queryable.Search(srchParams.SearchColumns, srchParams.SearchText);

                    }

                    //Sort
                    queryable = queryable.OrderByDynamic(srchParams.SortField, srchParams.SortOrder);
                    //Pagination
                    await queryable.InsertPaginationParametersInResponse(srchParams.RecordsPerPage, deals);
                    var paginatedDeals = queryable.Paginate(srchParams.CurrentPage, srchParams.RecordsPerPage).ToList();

                    deals.Records = paginatedDeals;
                    return deals;
                }
            }
            catch (Exception)
            {
                //Logger.LogError(ex.Message + "," + ex.StackTrace);
            }
            return null;
        }
        public async Task CreateUpdateDealExpenditure(List<DealExpenditure> lstExp)
        {
            using (var _dbContext = _factory.CreateDbContext())
            {
                foreach (var item in lstExp)
                {
                    if (!string.IsNullOrEmpty(item.Name))
                    {
                        if (string.IsNullOrEmpty(item.DisplayName))
                            item.DisplayName = item.Name;
                        if(item.IsOther)
                            item.DisplayName = item.Name;
                        if (item.ID == 0)//Add
                        {
                            _dbContext.DealExpenditure.Add(item);
                            await _dbContext.SaveChangesAsync();
                        }
                        else
                        {
                            var foundValue = await _dbContext.DealExpenditure.Where(s => s.ID == item.ID).AsNoTracking().FirstOrDefaultAsync();
                            if (foundValue != null)
                            {
                                foundValue.DealModelId = item.DealModelId;
                                foundValue.Name = item.Name;
                                foundValue.DisplayName = item.DisplayName;
                                foundValue.Value = item.Value;
                                foundValue.IsConfirmed = item.IsConfirmed;
                                foundValue.IsDeleted = item.IsDeleted;
                                foundValue.IsOther = item.IsOther;
                                _dbContext.Entry(foundValue).State = EntityState.Modified;
                                _dbContext.SaveChanges();
                            }
                        }
                    }
                }
            }
        }
        public async Task<List<DealExpenditure>?> GetDealExpenditureByDealID(string dealID)
        {
            try
            {
                using (var _dbContext = _factory.CreateDbContext())
                {
                    return await _dbContext.DealExpenditure.Where(e => e.IsDeleted == false && e.DealModelId == dealID).ToListAsync();
                }
            }
            catch (Exception)
            {
            }
            return null;
        }
        public async Task<Performance> GetPerformanceByDealId(string dealID)
        {
            try
            {
                using (var _dbContext = _factory.CreateDbContext())
                {
                    var perf = await _dbContext.Performance.Include(x => x.TopAccountList).FirstOrDefaultAsync(e => e.DealModelId == dealID);

                    if (perf == null)
                    {
                        return null;
                    }
                    else
                    {
                        return perf;
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
