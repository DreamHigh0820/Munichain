using Domain.Services.Database;
using Microsoft.AspNetCore.Components;
using Shared.Helpers;
using Shared.Models.Users;

namespace UI.Components.NavigationBar
{
    public partial class NavMenu
    {
        [Inject]
        public IFirmsService firmsService { get; set; }
        [Inject]
        public IUserService userService { get; set; }
        [Inject]
        public IDealParticipantService dealParticipantService { get; set; }
        private bool _menuVisible = false;
        private User user = new();
        private bool isFirmAdmin = false;
        private bool loading = true;
        private bool IsNotificationsPending = false;
        private void ToggleMenu()
        {
            _menuVisible = !_menuVisible;
        }

        protected override async Task OnInitializedAsync()
        {
            loading = true;
            var state = await authenticationStateProvider.GetAuthenticationStateAsync();
            if (state == null)
            {
                navigationManager.NavigateTo("/", true);
                return;
            }

            var userState = state?.ToUser();
            user = await userService.GetUserById(userState.Id);

            // If user exists in Graph but not in Munichain DB, create the user
            if (user == null)
            {
                userState.TimeZone = "Eastern Standard Time";
                userState.Confirmed = false;
                await userService.Create(userState);
                await dealParticipantService.UpdateParticipantForNewUser(userState);
                await firmsService.UpdateFirmMemberForNewUser(userState);
                //await emailService.NewSignupNotification(userState);
                navigationManager.NavigateTo("/", true);
                return;
            }

            isFirmAdmin = await firmsService.GetFirmWhereUserIsAdmin(user?.Email) != null;
            IsNotificationsPending = await notificationService.IsNotificationsPendingToRead(user);
            loading = false;
        }
    }
}
