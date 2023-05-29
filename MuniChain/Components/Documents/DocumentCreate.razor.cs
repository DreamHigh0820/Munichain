using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.SignalR.Client;
using UI.Components.Other;
using Shared.Helpers;
using Shared.Models.AppComponents;
using Shared.Models.DealComponents;
using Shared.Models.Enums;
using Shared.Models.Users;
using Shared.Validators;
using Syncfusion.Blazor.PdfViewer;
using Syncfusion.Blazor.PdfViewerServer;
using System.Text.Json;
using System.Text.Json.Serialization;
using JsonSerializer = System.Text.Json.JsonSerializer;
using Syncfusion.Blazor.DocumentEditor;
using System.ComponentModel;
using System.IO;
using ICSharpCode.SharpZipLib.Core;
using DocumentFormat.OpenXml.Bibliography;

namespace UI.Components.Documents
{
    public partial class DocumentCreate
    {
        [CascadingParameter]
        public Error? Error { get; set; }
        [Parameter]
        public string dealId { get; set; }
        public User user;
        public List<DealParticipant> dealParticipants;
        SfDocumentEditorContainer Container;
        public string documentName;
        public bool loading { get; set; } = true;
        protected override async Task OnInitializedAsync()
        {
            loading = true;
            var authState = await authenticationStateProvider.GetAuthenticationStateAsync();
            user = authState.ToUser();

            dealParticipants = await dealParticipantService.GetParticipantsByDealId(dealId);
            if (!dealParticipants.Select(x => x.EmailAddress).Contains(user.Email)) {
                navigationManager.NavigateTo("/404", true);
                return;
            }

            telemetry.Context.User.Id = user.Id;
            telemetry.TrackPageView("Document Create");
            loading = false;
        }

        public void OnLoad(object args)
        {
            Container.DocumentEditor.OpenBlankAsync();
            Container.EnableComment = false;
            Container.EnablePersistence = true;
        }

        public async Task OnSaveDocument()
        {
            var fileId = Guid.NewGuid().ToString();

            Document doc = new Document()
            {
                Id = fileId,
                DealId = dealId,
                IsSignature = false,
                CreatedDateTimeUTC = DateTime.UtcNow,
                PublicDocumentViewSettings = PublicDocumentViewSettings.Private,
                Name = documentName ?? "New Document",
                CreatedBy = user.Email,
                CreatedByFullName = user.DisplayName,
                UserPermissions = new List<string>() { }
            };

            string base64Data = await Container.DocumentEditor.SaveAsBlobAsync(FormatType.Docx);
            byte[] data = Convert.FromBase64String(base64Data);

            await documentService.Create(doc);
            await fileService.UploadFile(data, "word", dealId, fileId);

            navigationManager.NavigateTo($"/deal/{dealId}/documents");
        }
    }
}
