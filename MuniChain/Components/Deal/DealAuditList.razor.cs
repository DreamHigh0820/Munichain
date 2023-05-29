using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Components;
using Shared.Helpers;
using Shared.Models.DealComponents;
using Shared.Models.Users;
using UI.Components.Other;

namespace UI.Components.Deal
{
    public partial class DealAuditList
    {
        [CascadingParameter]
        public Error? Error { get; set; }
        [Parameter]
        public string dealId { get; set; }
        [Parameter]
        public User user { get; set; }
        [Parameter]
        public EventCallback<(string, bool)> ReloadDeal { get; set; }
        private bool loading = true;
        private bool DealDiffPopup = false;
        private List<ConcurrencyItem> dealDiffs = new();
        private List<DealModel> ChangeEvents = new();

        protected override async Task OnInitializedAsync()
        {
            loading = true;
            ChangeEvents = (await dealService.GetAuditDeals(dealId)).OrderByDescending(x => x.CreatedDateUTC).ToList();
            telemetry.Context.User.Id = user.Id;
            telemetry.TrackPageView("Deal Audit");
            loading = false;
        }

        public void CompareDeals(DealModel deal1, DealModel deal2)
        {
            if (deal2 == null || deal1 == null)
            {
                return;
            }
            else
            {
                DealDiffPopup = true;
                dealDiffs = deal1.ConcurrencyCompare(deal2).OrderBy(x => x.BaseModelName).ToList();
            }
        }

        public async Task LoadDeal(string id)
        {
            await ReloadDeal.InvokeAsync((id, true));
            return;
        }
    }


    public class DealChangeEvent
    {
        public string Id { get; set; }
        public string DealId { get; set; }
        public string Status { get; set; }
        public int Version { get; set; }
        public string LastModifiedBy { get; set; }
        public string LastModifiedByDisplayName { get; set; }
        public DateTime LastModifiedDateTime { get; set; }
    }

}
