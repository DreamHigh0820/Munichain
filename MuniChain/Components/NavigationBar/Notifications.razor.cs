using Syncfusion.Blazor.DropDowns;
using Shared.Models.AppComponents;
using Shared.Models.DealComponents;
using Shared.Models.Users;
using Shared.Helpers;

namespace UI.Components.NavigationBar
{
    public partial class Notifications
    {
        public List<NotificationMarkup> notifications { get; set; } = new List<NotificationMarkup>();
        public List<Notification> events { get; set; } = new();
        public User LoggedInUser;
        private DealModel currentDealFilter;
        private List<DealModel> deals = new();
        private bool loading = true;
        private bool FilterHidden = true;

        protected override async Task OnInitializedAsync()
        {
            loading = true;
            var authState = await authenticationStateProvider.GetAuthenticationStateAsync();
            LoggedInUser = authState.ToUser();
            var userFromDb = await userService.GetUserById(LoggedInUser.Id);

            if (userFromDb != null)
            {
                LoggedInUser.TimeZone = userFromDb.TimeZone;
            }
            await GetNotifications();
            await GetDeals();
            loading = false;
        }

        private async Task GetNotifications()
        {
            // Load all notification events
            events = (await notificationService.GetNotifications(LoggedInUser.Email)).Where(x => x.ActionBy != LoggedInUser.DisplayName).ToList();
            events.ForEach(x => notifications.Add(x.ToNotification()));
            notifications = notifications.Where(x => x != null).OrderByDescending(x => x.DateTimeUTC).ToList();
        }
        private async Task GetDeals()
        {
            deals = await dealService.GetByIds(events.Where(x => x.DealId != null && x.ActionBy != LoggedInUser.DisplayName).Select(x => x.DealId).Distinct().ToList());
        }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                //Update Notifications Read
                events.ForEach(x => x.IsUserRead = true);
                await notificationService.UpdateAll(events);
            }
        }
        private async Task ChangeDealFilter(ChangeEventArgs<DealModel, DealModel> e)
        {
            loading = true;
            await InvokeAsync(StateHasChanged);
            if (e?.Value?.Id == null)
            {
                currentDealFilter = e.Value;

                // Load all notifications
                notifications = new();
                events.ForEach(x => notifications.Add(x.ToNotification()));
                notifications = notifications.Where(x => x?.DateTimeUTC != null).OrderByDescending(x => x.DateTimeUTC).ToList();
            }
            else
            {
                // Load notification events for deal id
                currentDealFilter = e.Value;
                notifications = new();
                var eventsForDeal = events.Where(x => x.DealId != null && x.DealId == e.Value.Id).ToList();
                eventsForDeal.ForEach(x => notifications.Add(x.ToNotification()));
                notifications = notifications.Where(x => x?.DateTimeUTC != null).OrderByDescending(x => x.DateTimeUTC).ToList();
            }
            loading = false;
            await InvokeAsync(StateHasChanged);

        }
    }
}
