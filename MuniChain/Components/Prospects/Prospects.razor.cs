using Syncfusion.Blazor.DropDowns;
using Shared.Models.AppComponents;
using Shared.Models.DealComponents;
using Shared.Models.Users;
using Shared.Helpers;

namespace UI.Components.Prospects
{
    public partial class Prospects
    {
        public User user;
        private bool loading = true;

        protected override async Task OnInitializedAsync()
        {
            loading = false;
            return;
        }
    }
}
