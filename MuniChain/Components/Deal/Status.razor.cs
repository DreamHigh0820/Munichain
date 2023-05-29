using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using UI.Components.Other;
using Shared.Helpers;
using Shared.Models.DealComponents;
using Shared.Models.Users;
using Domain.Services.Database;

namespace UI.Components.Deal
{
    public partial class Status
    {
        [CascadingParameter]
        public Error? Error { get; set; }
        [Parameter]
        public DealModel dealInformation { get; set; }
        [Parameter]
        public bool CanEdit { get; set; }
        private bool dealStatusDropdownVisible = false;
        private User user;

        protected override async Task OnInitializedAsync()
        {
            var authState = await authenticationStateProvider.GetAuthenticationStateAsync();
            user = authState.ToUser();
            //Onetime set
        }

        private async Task ChangeStatus(string status)
        {
            try
            {
                dealStatusDropdownVisible = false;
                dealInformation.Status = status;
                await dealService.ChangeStatus(user, dealInformation);
            }
            catch (DbUpdateException ex)
            {
                toastService.ShowError("Someone has updated the deal after you have opened it. Please refresh");
                return;
            }
            catch (Exception ex)
            {
                Error?.ProcessError("Failed to change deal status", ex, user, dealInformation);
                return;
            }
        }

        private void ToggleStatus()
        {
            dealStatusDropdownVisible = !dealStatusDropdownVisible;
        }
    }
}
