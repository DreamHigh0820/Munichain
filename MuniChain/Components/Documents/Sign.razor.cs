using BoldSign.Api;
using BoldSign.Model;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using UI.Components.Other;
using Shared.Helpers;
using Shared.Models.DealComponents;
using Shared.Models.Users;
using Syncfusion.Blazor.DropDowns;
using Syncfusion.Blazor.Inputs;

namespace UI.Components.Documents
{
    public partial class Sign
    {
        [CascadingParameter]
        public Error? Error { get; set; }
        [Parameter]
        public string DealId { get; set; }
        public List<DealParticipant> dealParticipants { get; set; }
        private List<DocumentSigner> Recipients = new() { };
        private List<MunichainDocumentSigner> MunichainRecipients = new() { };
        private SfUploader uploader;
        private string AccountStatus { get; set; }
        private SenderIdentityViewModel BoldsignUser;
        private User user;
        private IBrowserFile browserFile;
        private string boldSignDocumentCreateUrl;
        [Inject]
        private IJSRuntime runtime { get; set; }
        private bool loading { get; set; }

        protected async override Task OnInitializedAsync()
        {
            loading = true;
            var state = await authenticationStateProvider.GetAuthenticationStateAsync();
            user = state.ToUser();
            telemetry.Context.User.Id = user.Id;
            telemetry.TrackPageView("Document Signature");

            try
            {
                BoldsignUser = await boldsignService.SearchUser(user.Email);
                AccountStatus = BoldsignUser?.Status.ToString() ?? "Unregistered";
                dealParticipants = await dealParticipantService.GetParticipantsByDealId(DealId);
            }
            catch (ApiException e)
            {
                // API Quota exceeded.
                if (e.ErrorCode == 429)
                {
                    Error.ProcessError("Boldsign rate limiting", e, user);
                    return;
                }
                else
                {
                    Error.ProcessError("Error loading boldsign", e, user);
                    return;
                }
            }
            catch (Exception e)
            {
                Error.ProcessError("Error loading boldsign", e, user);
                return;
            }
            loading = false;
        }

        public async Task OnSignUp()
        {
            try
            {
                var resp = await boldsignService.CreateUser(user.Email, user.DisplayName);
                AccountStatus = "Pending";
                StateHasChanged();
            }
            catch (Exception)
            {
                toastService.ShowError("Error creating Boldsign account, please contact Munichain support.");
            }
        }

        protected async Task UploadFile()
        {
            Recipients = new List<DocumentSigner>() { };
            MunichainRecipients.Where(x => !string.IsNullOrEmpty(x.Name) && !string.IsNullOrEmpty(x.Email)).ToList()
                .ForEach(x => Recipients.Add(new DocumentSigner(x.Name, x.Email, signerType: x.SignerType)));

            bool allExists = Recipients.All(x => dealParticipants.Any(y => x.EmailAddress == y.EmailAddress));

            if (allExists)
            {
                if (browserFile.Size > 1024 * 1024 * 25)
                {
                    toastService.ShowError("Max file size: 25MB");
                    return;
                }
                boldSignDocumentCreateUrl = await boldsignService.UploadFile(browserFile, BoldsignUser, DealId, Recipients);
                StateHasChanged();
            }
            else
            {
                // throw error
                toastService.ShowError(heading: "Error", message: "All recipients must be deal participants.");
                await uploader.ClearAllAsync();
                StateHasChanged();
            }
        }

        private async Task deleteRecipient(MunichainDocumentSigner recipient)
        {
            MunichainRecipients.Remove(recipient);
        }

        private async Task FillInFullName(ChangeEventArgs<string, DealParticipant> args)
        {
            var user = await userService.GetUserByEmail(args.ItemData.EmailAddress);
            if (user == null)
            {
                MunichainRecipients.FirstOrDefault(x => x.Email == args.ItemData.EmailAddress).Name = args.ItemData.EmailAddress;
            }
            else
            {
                MunichainRecipients.FirstOrDefault(x => x.Email == args.ItemData.EmailAddress).Name = user?.DisplayName;
            }
        }
        private async Task addRecipient()
        {
            MunichainRecipients.Add(new MunichainDocumentSigner() { SignerType = SignerType.Signer });
        }
        private async Task onReloadClick()
        {
            await runtime.InvokeAsync<object>("onReloadClick", null);
        }
        private async Task onSendClick()
        {
            await runtime.InvokeAsync<object>("onSendClick", new object[] { "" });
        }
    }
}
