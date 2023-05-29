using Domain.Services.Database;
using Microsoft.AspNetCore.Components;
using Shared.Helpers;
using Shared.Models.DealComponents;
using Shared.Models.Users;
using Syncfusion.Blazor.Schedule;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace UI.Components.Deal
{
    public partial class Schedule
    {
        [CascadingParameter]
        public Error? Error { get; set; }
        [Parameter]
        public DealModel dealInformation { get; set; }
        [Parameter]
        public List<string> currentUserPermissions { get; set; }
        private User user;
        private bool loading = true;
        private List<AppointmentData> DataSource = new();
        [Inject]
        public IAppointmentService appointmentService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            loading = true;
            var authState = await authenticationStateProvider.GetAuthenticationStateAsync();

            // Load saved appointments 

            user = authState.ToUser();

            telemetry.Context.User.Id = user.Id;
            telemetry.TrackPageView("Schedule");

            DataSource = await appointmentService.GetByDealId(dealInformation.Id);


            loading = false;
        }

        public async void OnActionBegin(ActionEventArgs<AppointmentData> args)
        {

            if (args.ActionType == ActionType.EventCreate)
            {
                var appt = args.AddedRecords.FirstOrDefault();
                appt.DealModelId = dealInformation.Id;
                var Create = await appointmentService.Create(appt);
                // Add appointment to database with deal id
            }
            else if (args.ActionType == ActionType.EventChange)
            {
                var appChange = args.ChangedRecords.FirstOrDefault();
                appChange.DealModelId = dealInformation.Id;
                // Update appointment
                var update = await appointmentService.Update(appChange);
            }
            else if (args.ActionType == ActionType.EventRemove)
            {
                var appRemove = args.DeletedRecords.FirstOrDefault();
                // Delete appointment
                var delete = await appointmentService.Delete(appRemove.Id);
            }
            else if (args.ActionType == ActionType.ViewNavigate)
            {

            }
        }
    }
}
