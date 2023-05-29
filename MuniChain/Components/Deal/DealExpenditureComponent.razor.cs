using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using UI.Components.Other;
using Shared.Helpers;
using Shared.Models.DealComponents;
using Shared.Models.Users;
using Syncfusion.Blazor.Inputs;
using Hangfire;

namespace UI.Components.Deal
{
    public partial class DealExpenditureComponent
    {
        [CascadingParameter]
        public Error? Error { get; set; }
        [Parameter]
        public User user { get; set; }
        [Parameter]
        public DealModel dealInformation { get; set; } = new();
        public List<DealParticipant> dealParticipantList { get; set; } = new();
        public bool IsEdit => !string.IsNullOrEmpty(dealInformation.Id);
        [Parameter]
        public bool CanCurrentUserWrite { get; set; }
        [Parameter]
        public List<string> UserPermissions { get; set; }
        private bool loading;
        [Inject]
        public IJSRuntime JsRuntime { get; set; }
        List<DealExpenditure> origExpenditures { get; set; } = new List<DealExpenditure>();
        List<DealExpenditure> LstDefaultExpenditures { get; set; } = new List<DealExpenditure>();
        DealExpenditure LastExpenditure => LstDefaultExpenditures.Where(e => e.IsDeleted == false).Last();

        protected override async Task OnInitializedAsync()
        {
            loading = true;
            var authState = await authenticationStateProvider.GetAuthenticationStateAsync();
            user = authState.ToUser();

            if (!UserPermissions.CanReadExpenditures())
            {
                navigationManager.NavigateTo("/404");
            }

            telemetry.Context.User.Id = user.Id;
            telemetry.TrackPageView("Expenditures");

            if (IsEdit)
            {
                LstDefaultExpenditures = await dealService.GetDealExpenditureByDealID(dealInformation.Id);
                if (!LstDefaultExpenditures.Any())
                {
                    // Seed database
                    AddDefaultExpenditures();
                    await dealService.CreateUpdateDealExpenditure(LstDefaultExpenditures);
                }
            }
            else
            {
                AddDefaultExpenditures();
            }
            //this sets an original expenditure list to compare changes to for notifications
            foreach (var exp in LstDefaultExpenditures)
            {
                origExpenditures.Add((DealExpenditure)exp.Clone());
            }
            loading = false;
            StateHasChanged();
            await JsRuntime.InvokeVoidAsync("stopScroll", "#numeric");
        }

        private void AddDefaultExpenditures()
        {
            LstDefaultExpenditures.Add(new DealExpenditure { DisplayName = "Underwriter's Counsel", Name = "UnderwritersCounsel", DealModelId = dealInformation.Id });
            LstDefaultExpenditures.Add(new DealExpenditure { DisplayName = "Bond Counsel", Name = "BondCounsel", DealModelId = dealInformation.Id });
            LstDefaultExpenditures.Add(new DealExpenditure { DisplayName = "Disclosure Counsel", Name = "DisclosureCounsel", DealModelId = dealInformation.Id });
            LstDefaultExpenditures.Add(new DealExpenditure { DisplayName = "Financial Advisor", Name = "FinancialAdvisor", DealModelId = dealInformation.Id });
            LstDefaultExpenditures.Add(new DealExpenditure { DisplayName = "Data Vendor", Name = "DataVendor", DealModelId = dealInformation.Id });
            LstDefaultExpenditures.Add(new DealExpenditure { DisplayName = "CUSIP Fee", Name = "CusipFee", DealModelId = dealInformation.Id });
            LstDefaultExpenditures.Add(new DealExpenditure { DisplayName = "DTC", Name = "DTC", DealModelId = dealInformation.Id });
            LstDefaultExpenditures.Add(new DealExpenditure { DisplayName = "Bond Insurance", Name = "BondInsurance", DealModelId = dealInformation.Id });
            LstDefaultExpenditures.Add(new DealExpenditure { DisplayName = "Pricing Advisor", Name = "PricingAdvisor", DealModelId = dealInformation.Id });
            LstDefaultExpenditures.Add(new DealExpenditure { DisplayName = "Rating Agency", Name = "RatingAgency", DealModelId = dealInformation.Id });
            LstDefaultExpenditures.Add(new DealExpenditure { DisplayName = "Road Show", Name = "RoadShow", DealModelId = dealInformation.Id });
            LstDefaultExpenditures.Add(new DealExpenditure { DisplayName = "Trustee/Paying Agent", Name = "TrusteeAgent", DealModelId = dealInformation.Id });
            LstDefaultExpenditures.Add(new DealExpenditure { DisplayName = "Document Hosting", Name = "DocumentHosting", DealModelId = dealInformation.Id });
            LstDefaultExpenditures.Add(new DealExpenditure { DisplayName = "Verification Agent", Name = "VerificationAgent", DealModelId = dealInformation.Id });
        }

        public decimal? Sum
        {
            get
            {
                var sum = LstDefaultExpenditures.Where(e => e.IsDeleted == false).Sum(e => e.Value);
                return sum;
            }
        }
        public async Task Submit()
        {
            try
            {
                if (CanCurrentUserWrite)
                {
                    await dealService.CreateUpdateDealExpenditure(LstDefaultExpenditures);
                    var newExpenditures = LstDefaultExpenditures.Where(x => !origExpenditures.Any(z => z.ID == x.ID)).ToList();

                    foreach (var exp in LstDefaultExpenditures.Where(x => !newExpenditures.Any(z => z.ID == x.ID)))
                    {
                        var match = origExpenditures.FirstOrDefault(x => x.ID == exp.ID);
                        List<ConcurrencyItem> diffs = exp.ConcurrencyCompare(match);
                        if (diffs.Any())
                        {
                            BackgroundJobs.Enqueue(() => notificationService.ExpendituresModifiedNotification(user, dealInformation, diffs, exp));
                        }
                    }
                    if (newExpenditures.Any())
                    {
                        foreach (var item in newExpenditures)
                        {
                            BackgroundJobs.Enqueue(() => notificationService.ExpenditureAddedNotification(user, dealInformation, item));
                        }
                    }

                    toastService.ShowSuccess(heading: "Saved", message: "Expenditures have been saved");
                }
                else
                {
                    toastService.ShowError(heading: "Error", message: "Expenditures cannot be saved");
                }

            }
            catch (Exception ex)
            {
                Error?.ProcessError("Failed to save expenditures", ex, user, dealInformation);
                return;
            }
        }
        public async Task AddNew()
        {
            try
            {
                LstDefaultExpenditures.Add(new DealExpenditure() { IsOther = true, DealModelId = dealInformation.Id });
                StateHasChanged();
                await Task.Delay(0);
                await JsRuntime.InvokeVoidAsync("stopScroll", "#numeric");
            }
            catch (Exception ex)
            {
                Error?.ProcessError("Failed to add new expenditure", ex, user, dealInformation);
                return;
            }
        }
        public async Task Remove(DealExpenditure expenditure)
        {
            try
            {
                expenditure.IsDeleted = true;
                StateHasChanged();
                await Task.Delay(0);
            }
            catch (Exception ex)
            {
                Error?.ProcessError("Failed to remove expenditures", ex, user, dealInformation);
                return;
            }
        }
        public async Task NameChange(DealExpenditure expenditure, ChangedEventArgs args)
        {
            try
            {
                expenditure.DisplayName = expenditure.Name = args.Value;
                StateHasChanged();
                await Task.Delay(0);
            }
            catch (Exception ex)
            {
                Error?.ProcessError("Failed to save name change on expenditure", ex, user, dealInformation);
                return;
            }
        }
    }
}
