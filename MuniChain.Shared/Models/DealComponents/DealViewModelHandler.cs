using Shared.Models.AppComponents;
using Shared.Models.Enums;
using Shared.Models.Users;

namespace Shared.Models.DealComponents
{
    public class DealViewRequest
    {
        public User User { get; set; }
        public string DealId { get; set; }
        public bool RequestingPublicCopy { get; set; }
    }

    public class DealViewResponse
    {
        public List<string> CurrentUserPermissions { get; set; }
        public bool CurrentUserIsParticipant { get; set; }
        public List<DealParticipant> UserParticipants { get; set; }
        public List<Firm> FirmParticipants { get; set; }
        public DealModel DealRequested { get; set; }
        public DealModel DealGranted { get; set; }
        public bool IsRequestedMasterDeal
        {
            get
            {
                return DealRequested?.HistoryDealID == null;
            }
        }
        public bool IsGrantedMasterDeal
        {
            get
            {
                return DealGranted?.HistoryDealID == null;
            }
        }
        public bool IsPublicView { get; set; }
        public DealViewType ViewType { get; set; }
        public List<DealExpenditure> Expenditures { get; set; }

        public List<Firm> AdvisorFirms
        {
            get
            {
                return FirmParticipants.Where(x => x.FirmType == FirmType.Advisor).ToList();
            }
        }
        public List<Firm> BondCounselFirms
        {
            get
            {
                return FirmParticipants.Where(x => x.FirmType == FirmType.BondCounsel).ToList();
            }
        }
    }
}
