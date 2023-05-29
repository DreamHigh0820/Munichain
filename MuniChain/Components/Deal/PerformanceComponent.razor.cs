using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using UI.Components.Other;
using Shared.Models.DealComponents;
using Shared.Helpers;
using Shared.Models.Users;
using Hangfire;

namespace UI.Components.Deal
{
    public partial class PerformanceComponent
    {
        [CascadingParameter]
        public Error? Error { get; set; }
        [Parameter]
        public User user { get; set; }
        [Parameter]
        public DealModel dealInformation { get; set; } = new();
        [Parameter]
        public List<string> UserPermissions { get; set; } = new();
        [Parameter]
        public bool CanCurrentUserWrite { get; set; } = false;
        [Inject]
        public IJSRuntime JsRuntime { get; set; }
        public List<TopAccount> origTopAccounts { get; set; } = new();
        private bool loading = true;

        Performance origPerformance;

        protected override async Task OnInitializedAsync()
        {
            loading = true;
            telemetry.Context.User.Id = user.Id;
            telemetry.TrackPageView("Performance");

            if (!UserPermissions.CanReadPerformance())
            {
                navigationManager.NavigateTo("/404");
            }

            dealInformation.Performance = await dealService.GetPerformanceByDealId(dealInformation.Id);
            if (dealInformation.Performance == null)
            {
                dealInformation.Performance = new Performance() { Id = Guid.NewGuid().ToString() };
            }
            origPerformance = (Performance)dealInformation.Performance.Clone();
            foreach (var account in origPerformance.TopAccountList)
            {
                origTopAccounts.Add((TopAccount)account.Clone());
            }
            loading = false;
        }

        public async Task onCreated(string id)
        {
            await JsRuntime.InvokeVoidAsync("stopScroll", id);
        }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await onCreated("#numeric");
            }
        }

        #region CRUD
        public async Task AddTopAccount()
        {
            if (dealInformation.Performance.TopAccountList == null || !dealInformation.Performance.TopAccountList.Any())
            {
                dealInformation.Performance.TopAccountList = new List<TopAccount> { };
            }

            dealInformation.Performance.TopAccountList?.Add(
                new TopAccount()
                {
                    Id = Guid.NewGuid().ToString(),
                    PerformanceId = dealInformation.Performance.Id,
                });
        }

        public async Task SavePerformance()
        {
            try
            {
                List<ConcurrencyItem> diffs = origPerformance.ConcurrencyCompare(dealInformation.Performance);
                if (diffs.Any())
                {
                    BackgroundJobs.Enqueue(() => notificationService.PerformanceChangedNotification(user, dealInformation, diffs));
                }
                List<ConcurrencyItem> accountComp = new List<ConcurrencyItem>();
                foreach (var account in dealInformation.Performance.TopAccountList)
                {
                    if (!origTopAccounts.Select(x => x.Id).Contains(account.Id))
                    {
                        BackgroundJobs.Enqueue(() => notificationService.PerformanceAccountChangedNotification(user, dealInformation, account, true, null));
                    }
                    else
                    {
                        accountComp = account.ConcurrencyCompare(origTopAccounts.First(x => x.Id == account.Id) ?? account);
                        if (accountComp.Any())
                        {
                            foreach (var variance in accountComp)
                            {
                                BackgroundJobs.Enqueue(() => notificationService.PerformanceAccountChangedNotification(user, dealInformation, account, false, diffs));
                            }
                        }
                    }
                }
                await dealService.UpdatePerformance(dealInformation.Performance);
                toastService.ShowSuccess(heading: "Saved", message: "Performance has been successfully saved");
            }
            catch (DbUpdateException)
            {
                toastService.ShowError("Someone has updated the deal after you have opened it. Please refresh");
                return;
            }
            catch (Exception ex)
            {
                Error?.ProcessError("Failed to save performance", ex, user, dealInformation);
                return;
            }
        }

        public async Task DeleteTopAccount(TopAccount topAccount)
        {
            dealInformation.Performance.TopAccountList?.Remove(topAccount);
        }
        #endregion
    }
}