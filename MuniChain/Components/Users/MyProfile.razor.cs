using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using UI.Components.Other;
using Shared.Helpers;
using Shared.Models.Enums;
using Shared.Models.Users;
using Syncfusion.Blazor.Inputs;
using Syncfusion.Blazor.Popups;

namespace UI.Components.Users
{
    public partial class MyProfile
    {
        [CascadingParameter]
        public Error? Error { get; set; }
        private User user;
        private string profilePictureUrl;
        private bool loading = true;
        public IEnumerable<string> States = Enum.GetNames(typeof(States));
        private bool IsCustomValueVisible { get; set; } = false;
        SfTextBox CustomValue;
        private List<string> ObjectToAddTo;
        private object ObjectToChange;
        private string PropToChange;
        private UserNotificationPreference notifSettings;
        public List<string> SeriesPurpose = new List<string>() { "Airports", "Appropriation", "Assisted Living Facilities", "Bond Anticipation Notes", "Bridge & Tunnel", "Building", "Capital Appreciation Bond", "Certificate of Obligation", "Charter School District", "County Government", "Dormitory Authority", "Economic Development", "Education", "Electric & Hydro Power", "Environmental", "Equipment Lease", "Escrow", "General Improvement", "Government Building", "Health Care", "Higher Education", "Highway", "Hospital", "Housing", "Industrial Development", "Law Enforcement", "Lease Revenue Bonds", "Multi Family Housing", "Municipal Utility District", "Non-Profit", "Parks & Recreation", "Pension Obligation", "Personal Income Tax", "Power", "Primary Education", "Prison", "Private University", "Public Improvement", "Public University", "Recreational Facilities", "Sales Tax", "Sanitation", "Secondary Education", "Single Family Housing", "Student Loans", "Tax Increment", "Tobacco Settlement", "Transportation", "University", "Utilities", "Various Purpose", "Water/Sewer/Gas", "501(c)(3)", "Other" };

        public List<string> JobTitles = new List<string>() { "Municipal Advisor", "Issuer", "Bond Counsel", "Underwriter", "Banker", "Paying Agent", "Rating Agency", "Back Office", "Bond Insurance", "Escrow Agent", "Trustee Agent", "Bank Counsel", "UW Counsel", "CPA", "Disclosure Counsel", "Other" };

        public delegate void MyDelegate(string input);
        public MyDelegate sendUpdates;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                loading = true;
                StateHasChanged();
                var authState = await authenticationStateProvider.GetAuthenticationStateAsync();
                var userId = authState.ToUser().Id;
                sendUpdates = new MyDelegate(AIStringChanged);
                user = await userService.GetUserById(userId);

                notifSettings = await notificationPreferences.Get(userId);

                profilePictureUrl = $"{user.BlobUrl}/{user.Id}?{UserExtensions.RandomString(3)}=";
                loading = false;
            }
            catch (Exception)
            {

                throw;
            }
            
        }

        private void AIStringChanged(string e)
        {
            user.Bio += e;
            StateHasChanged();
        }

        private async Task GenerateAI()
        {
            user.Bio = "";
            await aiService.GetBio(user?.Bio, user, sendUpdates);
        }

        protected async Task UploadProfilePicture(InputFileChangeEventArgs args)
        {
            var fileId = user.Id;
            await fileService.UploadFile(args.File, "profile-pictures", fileId);
            navigationManager.NavigateTo("/profile", true);
        }

        protected async Task UpdateUser()
        {
            try
            {
                await userService.UpdateUser(user);
                toastService.ShowSuccess(heading: "Saved", message: "User details saved successfully.");

            }
            catch (Exception ex)
            {
                Error?.ProcessError("Failed to update user", ex, user);
                return;
            }
        }

        protected async Task UpdateEmailNotification()
        {
            try
            {
                var CheckPrefreferenceByEmail = await notificationPreferences.Get(user.Id);
                if (CheckPrefreferenceByEmail == null)
                {
                    await notificationPreferences.Create(new UserNotificationPreference() { UserID = user.Id});
                    toastService.ShowSuccess(heading: "Saved", message: "Notification details saved successfully.");
                    return;
                }

                await notificationPreferences.Update(notifSettings);

                toastService.ShowSuccess(heading: "Saved", message: "Notification detail Updated successfully.");

            }
            catch (Exception ex)
            {
                Error?.ProcessError("Failed to update user", ex, user);
                return;
            }
        }

        private bool Visibility { get; set; } = false;
        DialogEffect animationEffect { get; set; } = DialogEffect.Zoom;
        private void OnProfileClick()
        {
            Visibility = true;
        }

        private void CustomValueChange(ChangeEventArgs change, List<string> values, User user, string prop)
        {
            // Select input changed, List of values needs to get updated after custom input
            ObjectToChange = user;
            PropToChange = prop;
            ObjectToAddTo = values;

            // If custom input show modal
            if (change.Value.ToString().Equals("Other"))
            {
                IsCustomValueVisible = true;
            }
            // If non-Other input just set property normally
            else
            {
                ObjectToChange.SetPropertyByName(prop, change.Value);
            }
        }

        private async Task CloseCustomInput()
        {
            // Add custom input to list of dropdown options
            ObjectToAddTo.Add(CustomValue.Value);
            // Set property to custom input value
            ObjectToChange.SetPropertyByName(PropToChange, CustomValue.Value);
            await emailService.CustomRoleInput(user.DisplayName, CustomValue.Value, user.Id);
            // Close
            IsCustomValueVisible = false;
        }
    }
}
