using Domain.Services.Database;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Routing;
using Shared.Helpers;
using Shared.Models.Users;

namespace UI.Components.Other
{
    public class TelemetryInsights : ComponentBase, IDisposable
    {
        [Inject]
        private TelemetryClient _telemetryClient { get; init; }

        [Inject]
        private NavigationManager _navigationManager { get; init; }
        [Parameter]
        public User user { get; set; }


        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender)
            {
                _navigationManager.LocationChanged += NavigationManagerOnLocationChanged;
            }
        }

        private void NavigationManagerOnLocationChanged(object? sender, LocationChangedEventArgs e)
        {
            _telemetryClient.Context.User.Id = user?.Id;
            _telemetryClient.TrackPageView(e.Location); //Set the argument to whatever you'd like to name the page

        }


        public void Dispose()
        {
            _navigationManager.LocationChanged -= NavigationManagerOnLocationChanged;
        }
    }
}
