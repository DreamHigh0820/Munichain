using Domain.Services.Database;
using Shared.Helpers;
using Shared.Models.DealComponents;
using Shared.Models.Enums;

namespace Domain.UIProvider
{
    public interface IDealViewUIProvider
    {
        public Task<DealViewResponse> GetView(DealViewRequest viewRequest);
        Task<DealViewResponse> ExportPdfRequirements(DealViewResponse view, ExportSettings settings, List<DealParticipant> participants, DealModel deal);
        bool ShowElement(DealViewResponse view, string tabName);
    }

    public class DealViewUIProvider : IDealViewUIProvider
    {
        private IDealService dealProvider;
        private IDealParticipantService dealParticipantsProvider;
        private IFirmsService firmProvider;

        public DealViewUIProvider(IDealService dealService, IDealParticipantService dealParticipantsProvider, IFirmsService firmService)
        {
            dealProvider = dealService;
            this.dealParticipantsProvider = dealParticipantsProvider;
            firmProvider = firmService;
        }

        // On Initialized
        public async Task<DealViewResponse> GetView(DealViewRequest viewRequest)
        {
            DealViewResponse viewResponse = new();

            // Get Requested Deal
            var requestedDeal = await dealProvider.GetById(viewRequest.DealId, DealViewType.ByID);
            if (requestedDeal == null)
            {
                viewResponse.IsPublicView = true;
                viewResponse.DealGranted = null;
                viewResponse.ViewType = DealViewType.NotFound;
                return viewResponse;
            }

            requestedDeal.HasBeenPublished = await dealProvider.HasBeenPublished(requestedDeal.Id, requestedDeal.HistoryDealID);
            requestedDeal.IsLatestPublished = await dealProvider.IsLatestPublished(requestedDeal.Id);
            viewResponse.DealRequested = requestedDeal;

            // If Requested deal is a history deal, we need to use the master Deal ID to find the participants
            viewResponse.UserParticipants = await dealParticipantsProvider.GetParticipantsByDealId(!viewResponse.IsRequestedMasterDeal ? requestedDeal?.HistoryDealID : viewRequest?.DealId);
            viewResponse.CurrentUserIsParticipant = viewResponse.UserParticipants.Exists(x => x.UserId == viewRequest?.User.Id);
            viewResponse.CurrentUserPermissions = viewResponse.UserParticipants?.FirstOrDefault(x => x.UserId == viewRequest?.User?.Id)?.DealPermissions;

            var viewType = await GetDealView(viewRequest, viewResponse);

            
            if (viewType.Item2 != null)
            {
                viewResponse.IsPublicView = viewType.Item1;
                viewResponse.DealGranted = viewType.Item2;
                viewResponse.DealGranted.HasBeenPublished = requestedDeal.HasBeenPublished;
                viewResponse.DealGranted.IsLatestPublished = await dealProvider.IsLatestPublished(viewType.Item2.Id);
            }
            else
            {
                viewResponse.IsPublicView = true;
                viewResponse.ViewType = DealViewType.NotFound;
            }

            return viewResponse;
        }

        // Which tab to show on Deal.razor SfTabs
        public bool ShowElement(DealViewResponse view, string tabName)
        {
            if (view == null || string.IsNullOrEmpty(tabName))
            {
                return false;
            }

            // Tabs
            if (tabName == "Information")
            {
                return view.IsPublicView && view.DealGranted.HasBeenPublished 
                    || view.CurrentUserPermissions.CanReadDeal();
            }
            else if (tabName == "Performance")
            {
                var isAudit = view.IsPublicView &&
                    view.CurrentUserIsParticipant &&
                    !view.IsRequestedMasterDeal &&
                    !view.DealRequested.IsLatestPublished &&
                    view.CurrentUserPermissions.IsOnlyDealAdmin();
                // IsAuditDeal
                return isAudit
                    || !view.IsPublicView && view.CurrentUserPermissions.CanReadPerformance();
            }
            else if (tabName == "Expenditures")
            {
                return !view.IsPublicView && view.CurrentUserPermissions.CanReadExpenditures();
            }
            else if (tabName == "Documents")
            {
                return (view.IsRequestedMasterDeal || view.DealGranted.IsLatestPublished) 
                    && view.DealGranted.Status != "Archived";
            }
            else if (tabName == "Calendar")
            {
                return !view.IsPublicView;
            }
            else if (tabName == "Message Board")
            {
                return !view.IsPublicView;
            }
            else if (tabName == "Audit")
            {
                return view.CurrentUserPermissions.IsOnlyDealAdmin() && !view.IsPublicView;
            }
            else if (tabName == "Signatures")
            {
                return !view.IsPublicView;
            }

            // Buttons
            else if (tabName == "CanEditStatus")
            {
                return view.DealGranted.Status != "Archived" 
                    && !view.IsPublicView 
                    && view.CurrentUserIsParticipant
                    && view.CurrentUserPermissions.IsOnlyDealAdmin() 
                    && view.IsGrantedMasterDeal;
            }
            else if (tabName == "LinkedInShare")
            {
                return view.DealGranted.HasBeenPublished && view.DealGranted.Status != "Archived";
            }
            else if (tabName == "Export")
            {
                return (!view.IsPublicView && view.CurrentUserPermissions.CanReadDeal())
                    || (view.IsPublicView && view.DealGranted.HasBeenPublished);
            }
            else if (tabName == "PrivateView")
            {
                return view.IsPublicView && view.CurrentUserIsParticipant && view.DealGranted?.Status != "Archived";
            }
            else if (tabName == "PublicView")
            {
                return !view.IsPublicView && view.DealGranted.Status != "Archived";
            }
            else if (tabName == "LockButton")
            {
                return !view.IsPublicView && view.CurrentUserPermissions.IsOnlyDealAdmin() && view.DealGranted.IsLocked != true;
            }
            else if (tabName == "UnlockButton")
            {
                return !view.IsPublicView && view.CurrentUserPermissions.IsOnlyDealAdmin() && view.DealGranted.IsLocked == true;
            }
            else if (tabName == "ArchiveButton")
            {
                return !view.IsPublicView && view.CurrentUserPermissions.IsOnlyDealAdmin() && view.DealGranted.Status != "Archived";
            }
            else if (tabName == "UnarchiveButton")
            {
                return view.IsGrantedMasterDeal && view.CurrentUserPermissions.IsOnlyDealAdmin() && view.DealGranted.Status == "Archived";
            }

            // Data
            else if (tabName == "LockedWarning")
            {
                return !view.CurrentUserPermissions.IsOnlyDealAdmin() && view.DealGranted.IsLocked == true;
            }

            return false;
        }

        // Get Deal View
        public Tuple<bool, DealViewType> GetDealViewModelForParticipant(bool HasBeenPublished, bool IsMasterDeal, bool IsLatestPublished, bool RequestingPublicCopy, List<string> CurrentUserPermissions)
        {
            if (!IsMasterDeal)
            {
                // If latest published deal, anybody can see
                if (IsLatestPublished)
                {
                    return new Tuple<bool, DealViewType>(true, DealViewType.ByID);
                }
                else
                {
                    // Only deal admins can see historical deals
                    if (CurrentUserPermissions.IsOnlyDealAdmin())
                    {
                        return new Tuple<bool, DealViewType>(true, DealViewType.ByID);
                    }
                    else
                    {
                        return new Tuple<bool, DealViewType>(true, DealViewType.NotFound);
                    }
                }
            }
            else
            {
                if (RequestingPublicCopy)
                {
                    if (HasBeenPublished)
                    {
                        return new Tuple<bool, DealViewType>(true, DealViewType.LatestPublished);
                    }
                    else
                    {
                        if (CurrentUserPermissions.CanReadDeal())
                        {
                            return new Tuple<bool, DealViewType>(true, DealViewType.ByID);
                        }
                        else
                        {
                            return new Tuple<bool, DealViewType>(true, DealViewType.DealReadFalse);
                        }
                    }
                }
                else
                {
                    if (CurrentUserPermissions.CanReadDeal())
                    {
                        return new Tuple<bool, DealViewType>(false, DealViewType.ByID);
                    }
                    else
                    {
                        return new Tuple<bool, DealViewType>(false, DealViewType.DealReadFalse);
                    }
                }
            }

        }

        public async Task<Tuple<bool, DealViewType?>> GetDealViewModelForNonParticipant(bool RequestedPublicDeal, DealModel deal)
        {
            return new Tuple<bool, DealViewType?>(true, DealViewType.LatestPublished);
        }

        private async Task<Tuple<bool, DealModel?>> GetDealView(DealViewRequest viewRequest, DealViewResponse viewResponse)
        {
            Tuple<bool, DealViewType?> nonParticipantResults;

            if (!viewResponse.CurrentUserIsParticipant)
            {
                nonParticipantResults = await GetDealViewModelForNonParticipant(viewResponse.DealRequested.IsLatestPublished, viewResponse.DealRequested);
                return new Tuple<bool, DealModel?>(nonParticipantResults.Item1, await dealProvider.GetById(viewRequest.DealId, nonParticipantResults.Item2));
            }
            else
            {
                if (viewResponse?.DealRequested.Status == "Archived" || viewResponse?.DealRequested?.IsLocked == true)
                {
                    return await HandleLockedAndArchived(viewRequest, viewResponse);
                }

                var DealViewModel = GetDealViewModelForParticipant(
                    viewResponse.DealRequested.HasBeenPublished,
                    viewResponse.IsRequestedMasterDeal,
                    viewResponse.DealRequested.IsLatestPublished,
                    viewRequest.RequestingPublicCopy,
                    viewResponse.CurrentUserPermissions);

                return new Tuple<bool, DealModel?>(DealViewModel.Item1, await dealProvider.GetById(viewRequest.DealId, DealViewModel.Item2));
            }
        }

        public async Task<Tuple<bool, DealModel?>> HandleLockedAndArchived(DealViewRequest viewRequest, DealViewResponse viewResponse)
        {
            // Archived logic
            if (viewResponse?.DealRequested?.Status == "Archived")
            {
                if (viewResponse.CurrentUserPermissions.IsOnlyDealAdmin() && viewResponse.IsRequestedMasterDeal)
                {
                    // If you are admin, show public view Archived deal
                    var deal = await dealProvider.GetById(viewRequest.DealId, DealViewType.ByID);
                    return new Tuple<bool, DealModel>(true, deal);
                }
                else
                {
                    var deal = await dealProvider.GetById(viewRequest.DealId, DealViewType.NotFound);
                    // Message saying error
                    return new Tuple<bool, DealModel>(true, deal);
                }
            }
            else
            {
                if (viewRequest.RequestingPublicCopy == true)
                {
                    if (viewResponse.DealRequested.HasBeenPublished)
                    {
                        var deal = await dealProvider.GetById(viewRequest.DealId, DealViewType.LatestPublished);
                        return new Tuple<bool, DealModel>(true, deal);
                    }
                    else
                    {
                        var deal = await dealProvider.GetById(viewRequest.DealId, DealViewType.Latest);
                        return new Tuple<bool, DealModel>(true, deal);
                    }
                }
                else
                {
                    if (viewResponse.CurrentUserPermissions.IsOnlyDealAdmin())
                    {
                        // Deal admins are able to edit the deal in locked mode
                        var deal = await dealProvider.GetById(viewRequest.DealId, DealViewType.Latest);
                        return new Tuple<bool, DealModel>(false, deal);
                    }
                    else
                    {
                        if (!viewResponse.IsRequestedMasterDeal)
                        {
                            return new Tuple<bool, DealModel>(true, null);
                        }

                        var CanViewLockedDealInformation = viewResponse.IsRequestedMasterDeal && viewResponse.CurrentUserPermissions.CanReadDeal() ? DealViewType.Latest : DealViewType.DealReadFalse;
                        var deal = await dealProvider.GetById(viewRequest.DealId, CanViewLockedDealInformation);
                        return new Tuple<bool, DealModel>(false, deal);
                    }
                }
            }
        }

        // Export PDF Feature
        public async Task<DealViewResponse> ExportPdfRequirements(DealViewResponse view, ExportSettings settings, List<DealParticipant> participants, DealModel deal)
        {
            var firmIds = (await firmProvider.GetFirmsByUserEmail(participants.Select(x => x.EmailAddress).ToArray())).Select(x => x.FirmId);
            view.FirmParticipants = await firmProvider.GetByIds(firmIds.ToList());

            if (settings.Expenditures)
            {
                view.Expenditures = await dealProvider.GetDealExpenditureByDealID(deal.Id);
            }

            return view;
        }

    }
}
