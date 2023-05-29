using Microsoft.AspNetCore.Components;
using Shared.Models.Users;

namespace UI.Components.Users
{
    public partial class Advisor
    {
        [Parameter]
        public string Id { get; set; }
        private User user;
        private bool loading { get; set; } = true;

        protected override async Task OnInitializedAsync()
        {
            loading = true;
            user = await userService.GetUserById(Id);

            if (user == null)
            {
                navigationManager.NavigateTo("/404", true);
                return;
            }

            user.AssociatedFirm = await firmService.GetFirmForUser(user.Email);

            loading = false;
        }
    }
}
