using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Syncfusion.Blazor.Navigations;
using Shared.Models.DealComponents;
using Domain.Services.ThirdParty;
using Shared.Models.Enums;
using Shared.Helpers;
using Shared.Models.Users;
using UI.Components.Other;
using Microsoft.AspNetCore.SignalR.Client;
using Hangfire;
using Microsoft.EntityFrameworkCore;

namespace UI.Components.Deal
{
    public partial class Deal
    {
        #region Vars
        [CascadingParameter]
        public Error? Error { get; set; }
        [Parameter]
        public string dealId { get; set; }
        [Parameter]
        public string Select { get; set; }

        DealViewResponse view;
        public DealModel dealInformation;
        public bool loading = true;
        public string Referral;
        public User user;

        SfTab Tabs;
        public int SelectedIndex = 0;

        public ExportSettings exportSettings = new();
        public string PublishHelpContent = "Publishing this deal makes the deal information public on the Munichain platform. Only Public Documents will be available to non-participants. Shared Documents will stay private.";
        public bool ExportDeal = false;
        private bool ArchiveDealPopup { get; set; } = false;
        #endregion

        protected override async Task OnInitializedAsync()
        {
            loading = true;
            var state = await authenticationStateProvider.GetAuthenticationStateAsync();
            user = state.ToUser();

            var uri = navigationManager.ToAbsoluteUri(navigationManager.Uri);

            if (QueryHelpers.ParseQuery(uri.Query).TryGetValue("ref", out var param))
            {
                Referral = param;
            }

            try
            {
                // If you're coming from deals table or /public we set RequestingPublicCopy to true
                await LoadComponent(dealId, Referral?.ToLower() == "deals" || Select?.ToLower() == "public");
            }
            catch (Exception ex)
            {
                Error?.ProcessError($"Failed to load deal by ID {dealId}", ex, user);
                return;
            }

            dealInformation = view.DealGranted;
            loading = false;
        }

        public async Task LoadComponent(string id, bool RequestingPublicCopy)
        {
            loading = true;
            StateHasChanged();

            view = await uiProvider.GetView(new DealViewRequest()
            {
                User = user,
                RequestingPublicCopy = RequestingPublicCopy,
                DealId = id,
            });

            if (view?.ViewType == DealViewType.NotFound)
            {
                navigationManager.NavigateTo("/404", true);
                return;
            }
            dealInformation = view.DealGranted;

            if (view.IsGrantedMasterDeal || view.DealGranted.IsLatestPublished)
            {
                var url = view.DealGranted.IsLatestPublished ? $"/deal/{dealInformation.HistoryDealID}" : $"/deal/{dealInformation.Id}";
                if (RequestingPublicCopy)
                {
                    url += "/public";
                }
                if (!string.IsNullOrEmpty(Select) && Select?.ToLower() != "public")
                {
                    url += $"/{Select}";
                }
                navigationManager.NavigateTo(url);
            }
            else
            {
                navigationManager.NavigateTo($"/deal/{dealInformation.Id}");
            }

            loading = false;
            StateHasChanged();
        }

        public async Task ExportPdfAsync(ExportSettings settings)
        {
            loading = true;
            ExportDeal = false;
            await InvokeAsync(() => StateHasChanged());

            view = await uiProvider.ExportPdfRequirements(view, exportSettings, view.UserParticipants, dealInformation);

            using (MemoryStream excelStream = PdfExportService.CreatePdf(settings, dealInformation, view.AdvisorFirms.Select(x => x.Name).ToList(), view.BondCounselFirms.Select(x => x.Name).ToList(), view.Expenditures))
            {
                if (excelStream == null)
                {
                    return;
                }

                await JS.SaveAs($"{dealInformation.Issuer}-{DateTime.Now.ToShortDateString()}.pdf", excelStream.ToArray());
            }
            loading = false;
            await InvokeAsync(() => StateHasChanged());
        }

        void SetTabsOnLoad()
        {
            if (Tabs.Items.Count > 0)
            {
                if (Select == null || Select.ToLower() == "public")
                {
                    SelectedIndex = 0;
                    return;
                }

                int index = Tabs.Items.FindIndex(x => x.Header.Text.ToLower().StartsWith(Select.ToLower()));
                if (index == null || index == -1)
                {
                    SelectedIndex = 0;
                    return;
                }

                SelectedIndex = index;
            }
        }

        public async Task LockDeal(bool isLocked)
        {
            try
            {
                dealInformation.IsLocked = isLocked;
                await dealService.LockUnlockDeal(dealInformation);
                await LoadComponent(dealInformation.Id, false);
                return;
            }
            catch (DbUpdateException)
            {
                toastService.ShowError("Someone has updated the deal after you have opened it. Please refresh");
                return;
            }
            catch (Exception ex)
            {
                Error?.ProcessError("Deal failed to lock", ex, user, dealInformation);
                return;
            }
        }

        public async Task ArchiveDeal()
        {
            if (dealInformation.Status == "Archived")
            {
                return;
            }

            try
            {
                ArchiveDealPopup = false;
                dealInformation.Status = "Archived";
                await dealService.ChangeStatus(user, dealInformation);
                await LoadComponent(dealInformation.Id, false);
                BackgroundJobs.Enqueue(() => notificationService.DealArchivedNotification(dealInformation, user));

                return;
            }
            catch (DbUpdateException)
            {
                toastService.ShowError("Someone has updated the deal after you have opened it. Please refresh");
                return;
            }
            catch (Exception ex)
            {
                Error?.ProcessError("Deal failed to archive", ex, user, dealInformation);
                return;
            }
        }

        public async Task UnarchiveDeal()
        {
            try
            {
                await dealService.UnArchiveAllDeals(user, dealInformation);
                await LoadComponent(dealInformation.Id, false);
            }
            catch (Exception ex)
            {
                Error?.ProcessError("Deal failed to archive", ex, user, dealInformation);
                return;
            }
        }

        private bool actionsDropdownVisible = false;
        private void ToggleActions()
        {
            actionsDropdownVisible = !actionsDropdownVisible;
        }

    }
}
