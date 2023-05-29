using Microsoft.AspNetCore.Components;
using Shared.Models.DealComponents;
using Shared.Models.Users;

namespace UI.Components.Users
{
    public partial class ReferencesAdd
    {
        [Parameter]
        public DealModel dealInformation { get; set; }
        public Reference referenceToAdd = new();
        public bool referenceGiven;
        public User user;

        protected override async Task OnInitializedAsync()
        {
            /*
            var state = await authenticationStateProvider.GetAuthenticationStateAsync();
            user = state.ToUser();

            referenceGiven = references.Exists(x => x.GivenBy == user.Email);

            if (referenceGiven)
            {
                referenceToAdd = references.First(x => x.GivenBy == user.Email);
            }
            */
        }

        protected async Task OnSubmit()
        {
            /*
            referenceToAdd.DealId = dealInformation.Id;
            referenceToAdd.GivenBy = user.Email;
            referenceToAdd.DateGiven = DateTime.Now;
            referenceToAdd.FirmId = dealInformation.AdvisorFirm;

            if (referenceGiven)
            {
                await referenceService.Update(referenceToAdd);
            }
            else
            {
                await referenceService.Create(referenceToAdd);
            }
            */
        }
    }
}
