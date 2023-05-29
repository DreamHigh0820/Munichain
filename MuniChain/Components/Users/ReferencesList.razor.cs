using Microsoft.AspNetCore.Components;
using Shared.Helpers;
using Shared.Models.DealComponents;
using Shared.Models.Enums;
using Shared.Models.Users;

namespace UI.Components.Users
{
    public partial class ReferencesList
    {

        [Parameter]
        public List<Reference> references { get; set; }
        public User user;

        protected override async Task OnInitializedAsync()
        {
            var state = await authenticationStateProvider.GetAuthenticationStateAsync();
            user = state.ToUser();
            foreach (var reference in references)
            {
                var deal = await dealService.GetById(reference?.DealId, DealViewType.ByID);
                reference.DealName = deal.Issuer;
                var givenBy = await userService.GetUserByEmail(reference.GivenBy);
                reference.UserRole = givenBy.JobTitle;
            }
        }
    }
}
