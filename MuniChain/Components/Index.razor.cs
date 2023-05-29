using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.JSInterop;
using UI.Components.Other;
using Shared.Models.DealComponents;
using Domain.Services.Database;
using Shared.Models.Enums;
using Shared.Models.Users;
using Shared.Helpers;
using Shared.Models.AppComponents;
using Microsoft.AspNetCore.Components.Authorization;
using Hangfire;
using Domain.Services;

namespace UI.Components
{
    public partial class Index
    {
        [CascadingParameter]
        public Error? Error { get; set; }
        [Inject]
        public IDealService dealService { get; set; }
        [Inject]
        public IFirmsService firmsService { get; set; }
        [Inject]
        public IDealParticipantService dealParticipantService { get; set; }
        [Inject]
        public IUserService userService { get; set; }
        [Inject]
        public IJSRuntime JsRuntime { get; set; }

        private bool loading { get; set; } = true;
        private bool ShowCreateDealPopup { get; set; } = false;
        private string documentId;
        public DealModel createDeal = new();
        public string[] EnumValues = Enum.GetNames(typeof(States));
        public List<Firm> IssuerFirms = new();
        private User user;

        protected override async Task OnInitializedAsync()
        {
            loading = true;
            RecurringJobs.AddOrUpdate<NotificationGrouper>("MuniNotification-Grouper", x => x.SendEmails(), Cron.MinuteInterval(2));

            var authState = await authenticationStateProvider.GetAuthenticationStateAsync(); /*await AuthenticationStateProvider.GetAuthenticationStateAsync();*/
            user = authState.ToUser();
            var uri = navigationManager.ToAbsoluteUri(navigationManager.Uri);

            if (QueryHelpers.ParseQuery(uri.Query).TryGetValue("documentId", out var param))
            {
                documentId = param;

                if (documentId != null)
                {
                    navigationManager.NavigateTo($"/document/{documentId}");
                    return;
                }
            }

            try
            {
                var userFromDb = await userService.GetUserById(user.Id);
                if (userFromDb != null)
                {
                    user = userFromDb;
                }
            }
            catch (Exception ex)
            {
                Error?.ProcessError("Index page could not render", ex, user);
                return;
            }

            loading = false;
        }

        public async Task ShowCreateDealModal()
        {
            createDeal = new();
            ShowCreateDealPopup = true;
        }

        public async Task CreateDeal()
        {
            createDeal = createDeal.SetDefaults(user);
            createDeal.Id = Guid.NewGuid().ToString();
            var participant = new DealParticipant()
            {
                DealId = createDeal.Id,
                EmailAddress = user.Email,
                IsPublic = true,
                Id = Guid.NewGuid().ToString(),
                DisplayName = user.DisplayName,
                UserId = user.Id,
                Role = user.JobTitle,
                DateAddedUTC = DateTime.Now.ToUniversalTime(),
                DealPermissions = new List<string> { PermissionExtensions.DealAdmin }
            };

            try
            {
                await dealService.Create(createDeal);
                var firm = await firmsService.GetByName(createDeal.Issuer);
                // Auto add deal creator to participants list
                await dealParticipantService.CreateParticipant(participant);

                navigationManager.NavigateTo($"/deal/{createDeal.Id}");
                return;
            }
            catch (Exception)
            {
                ShowCreateDealPopup = false;
                toastService.ShowError(heading: "Error", message: "Deal failed to create.");
            }

        }

        public async Task onCreated(string id)
        {
            await JsRuntime.InvokeVoidAsync("stopScroll", id);
        }
    }
}
