using Microsoft.AspNetCore.Components;
using Shared.Models.AppComponents;
using Shared.Models.Users;

namespace UI.Components.Firms
{
    public partial class FirmView
    {
        [Parameter]
        public string firmId { get; set; }
        public Firm firm { get; set; }
        public List<User> users { get; set; } = new();
        public Tuple<int, decimal?> Summary { get; set; }
        public bool seeMore = false;
        public bool loading { get; set; } = true;
        private bool AdditionalMembersPopup;

        protected override async Task OnInitializedAsync()
        {
            loading = true;
            firm = await firmsService.GetById(firmId);
            if (firm == null) { navigationManager.NavigateTo("/404", true); return; }

            var usersFromGraph = await userService.BatchGetUsersByEmail(firm?.Members.Select(x => x.EmailAddress).ToArray(), 10);
            if (await userService.CountFirmMembers(firmId) > 10)
            {
                seeMore = true;
            }

            foreach (var firmMember in firm?.Members)
            {
                var registeredUser = usersFromGraph.FirstOrDefault(x => x.Email == firmMember.EmailAddress) ?? null;
                if (registeredUser == null)
                {
                    users.Add(new User() { Email = firmMember.EmailAddress, DisplayName = "Unregistered User", });
                }
                else
                {
                    users.Add(registeredUser);
                }
            }

            Summary = await dealService.GetDealsSumByFirmID(firm.Id, true);
            loading = false;
        }

        public async Task SeeMore()
        {
            AdditionalMembersPopup = true;
        }
    }
}
