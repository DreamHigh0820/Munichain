using Domain.Services.Database;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Rendering;
using Shared.Helpers;

namespace UI.Components.Other
{
    public class ConfirmedAuthorizeView : AuthorizeView
    {
        private bool isAuthorized = false;
        private AuthenticationState state;
        [Inject]
        private AuthenticationStateProvider AuthenticationStateProvider { get; set; }
        [Inject]
        private IUserService userService { get; set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (isAuthorized)
            {
                builder.AddContent(1, ChildContent, state);
            }
        }

        protected override async Task OnParametersSetAsync()
        {
            var userId = (await AuthenticationStateProvider.GetAuthenticationStateAsync()).ToUser()?.Id;
            if (userId == null)
            {
                isAuthorized = false;
                return;
            }
            var confirmed = (await userService.GetUserById(userId))?.Confirmed;
            isAuthorized = confirmed == true;
        }
    }
}
