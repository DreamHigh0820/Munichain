using Azure.Storage.Blobs.Models;
using BoldSign.Model;
using Hangfire;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using Shared.Helpers;
using Shared.Models.DealComponents;
using Shared.Models.Enums;
using Shared.Models.Users;
using Shared.Validators;
using Syncfusion.Blazor.Inputs;
using System.Reflection;
using UI.Components.Other;
using Document = Shared.Models.DealComponents.Document;

namespace UI.Components.Documents
{
    public partial class Upload
    {
        [CascadingParameter]
        public Error? Error { get; set; }
        [Parameter]
        public DealModel dealInformation { get; set; }
        [Parameter]
        public bool? IsPublicView { get; set; }
        public List<DealParticipant> dealParticipants { get; set; }
        [Parameter]
        public bool IsEdit { get; set; } = true;
        private User user;
        private List<BlobItem> files = new List<BlobItem>();
        private List<BehalfDocument> sharedDocuments = new();
        private List<Document> documents = new List<Document>();
        private Progress<int> progressHandler;
        private IBrowserFile browserFile;
        private int ProgressPercent;
        public bool loading { get; set; } = true;
        public bool uploading { get; set; } = false;
        private bool EditVisibility { get; set; } = false;

        public Document documentToUpload = new() { PublicDocumentViewSettings = PublicDocumentViewSettings.Private };
        public Document documentToEdit = new();
        public Document documentToDelete = new();
        public Document documentToRevoke = new();

        #region CustomValue
        SfTextBox CustomValue;
        private Guid inputFileId = Guid.NewGuid();
        private bool IsCustomValueVisible { get; set; } = false;
        private List<string> ObjectToAddTo;
        private object ObjectToChange;
        private string PropToChange;
        private string Custom { get; set; }
        public List<string> DocumentTypes { get; set; } = new List<string>() { "POS", "OS", "BPA", "Other" };
        #endregion

        protected override async Task OnInitializedAsync()
        {
            loading = true;
            progressHandler = new Progress<int>(UploadProgressChanged);
            var state = await authenticationStateProvider.GetAuthenticationStateAsync();
            user = state.ToUser();

            telemetry.Context.User.Id = user.Id;
            telemetry.TrackPageView("Document Upload");

            var userFromDb = await userService.GetUserById(user.Id);

            if (userFromDb != null)
            {
                user.TimeZone = userFromDb.TimeZone;
            }

            var dealId = dealInformation.IsMasterCopy ? dealInformation.Id : dealInformation.HistoryDealID;
            dealParticipants = await dealParticipantService.GetParticipantsByDealId(dealId);
            await LoadDocuments(user.Email, dealId);

            loading = false;
        }

        private void UploadProgressChanged(int progress)
        {
            ProgressPercent = progress;
            StateHasChanged();
        }

        public void Dispose()
        {
        }

        private async Task LoadDocuments(string email, string dealId)
        {
            documents = new List<Document>();
            sharedDocuments = (await boldsignService.ListDocuments(email, dealId)).Where(x => x.Status != DocumentStatus.Revoked).ToList();
            var hasBeenPublished = await dealService.HasBeenPublished(dealId, null);
            // Load all files in container
            files = await fileService.GetFiles(dealId);
            foreach (var file in files.ToList())
            {
                var doc = await documentService.GetById(file.Name);
                if (doc != null)
                {
                    if (DocumentPermissionValidator.Validate(doc, dealParticipants, user, IsPublicView == true, hasBeenPublished))
                    {
                        documents.Add(doc);
                    }
                }
            }

            sharedDocuments.ForEach(doc => documents.Add(new Document()
            {
                IsSignature = true,
                DealId = dealId,
                CreatedDateTimeUTC = DateTimeOffset.FromUnixTimeSeconds(doc.CreatedDate).DateTime.ToUniversalTime(),
                CreatedBy = doc.BehalfOf.EmailAddress,
                CreatedByFullName = doc.BehalfOf.Name,
                Status = doc.Status.ToString(),
                Id = doc.DocumentId,
                Name = doc.MessageTitle,
            }));

            if (IsPublicView == false)
            {
                documents = documents.OrderByDescending(x => x.CreatedDateTimeUTC).ToList();
            }
            else
            {
                documents = documents.Where(x => x.PublicDocumentViewSettings == PublicDocumentViewSettings.Public).OrderByDescending(x => x.CreatedDateTimeUTC).ToList();
            }
        }

        protected async Task UploadFile()
        {
            uploading = true;
            var file = browserFile;
            if (documentToUpload.PublicDocumentViewSettings != PublicDocumentViewSettings.Custom)
            {
                documentToUpload.UserPermissions = new List<string>();
            }
            else
            {
                if (documentToUpload.UserPermissions == null || !documentToUpload.UserPermissions.Any())
                {
                    toastService.ShowError("You must include participants when selecting the Custom permissions.");
                    return;
                }
            }

            var fileId = Guid.NewGuid().ToString();
            var docToSave = new Document()
            {
                Id = fileId,
                Name = !string.IsNullOrEmpty(documentToUpload.Name) ? documentToUpload.Name : file.Name,
                CreatedBy = user.Email,
                CreatedByFullName = user.DisplayName,
                DealId = dealInformation.Id,
                CreatedDateTimeUTC = DateTime.UtcNow,
                PublicDocumentViewSettings = documentToUpload.PublicDocumentViewSettings,
                Type = !string.IsNullOrEmpty(documentToUpload.Type) ? documentToUpload.Type : "Other",
                UserPermissions = documentToUpload.UserPermissions
            };

            await documentService.Create(docToSave);
            await fileService.UploadFile(file, dealInformation.Id, fileId, progressHandler: progressHandler);
            BackgroundJobs.Enqueue(() => notificationService.DocUploadNotification(docToSave, user, dealInformation));
            await LoadDocuments(user.Email, dealInformation.Id);
            uploading = false;
            ProgressPercent = 0;
            browserFile = null;
            documentToUpload = new() { PublicDocumentViewSettings = PublicDocumentViewSettings.Private };
            inputFileId = Guid.NewGuid();
            StateHasChanged();
        }

        private async Task OnEditClick(Document doc, bool first = false)
        {
            // Need to make sure that user can set the current doc permission
            try
            {
                if (!first)
                {
                    if (documentToEdit.PublicDocumentViewSettings != PublicDocumentViewSettings.Custom)
                    {
                        documentToEdit.UserPermissions = new List<string>();
                    }
                    else
                    {
                        if (documentToEdit.UserPermissions == null || !documentToEdit.UserPermissions.Any())
                        {
                            toastService.ShowError("You must include participants when selecting the Custom permissions.");
                            return;
                        }
                    }

                    await documentService.Update(documentToEdit);
                    BackgroundJobs.Enqueue(() => notificationService.DocVisibilityChangedNotification(documentToEdit, user, dealInformation, documentToEdit.PublicDocumentViewSettings));
                    EditVisibility = false;
                    await LoadDocuments(user.Email, dealInformation.Id);
                    return;
                }

                documentToEdit = (Document)doc.Clone();
                EditVisibility = true;
            }
            catch (DbUpdateException)
            {
                toastService.ShowError("Someone has updated the deal after you have opened it. Please refresh");
                return;
            }
            catch (Exception ex)
            {
                Error?.ProcessError("Failed edit document", ex, user, dealInformation);
                return;
            }

        }

        private bool DeleteVisibility { get; set; } = false;
        private async Task OnDeleteClick(Document doc, bool first = false)
        {
            if (!first)
            {
                await documentService.DeleteById(doc.Id);
                await fileService.DeleteFile(dealInformation.Id, doc.Id);
                DeleteVisibility = false;
                BackgroundJobs.Enqueue(() => notificationService.DocDeleteNotification(doc, user, dealInformation));
                await LoadDocuments(user.Email, dealInformation.Id);
                return;
            }

            documentToDelete = (Document)doc.Clone();
            DeleteVisibility = true;
        }
        private bool RevokeVisibility { get; set; } = false;

        private async Task OnRevokeClick(Document doc, bool first = false)
        {
            if (!first)
            {
                await boldsignService.RevokeDocument(doc.Id, user.Email, user.DisplayName);
                RevokeVisibility = false;
                await LoadDocuments(user.Email, dealInformation.Id);
                return;
            }

            documentToRevoke = (Document)doc.Clone();
            RevokeVisibility = true;
        }

        #region Custom Input for Dropdown
        private void CustomValueChange(ChangeEventArgs change, List<string> values, Document document, string prop)
        {
            // Select input changed, List of values needs to get updated after custom input
            ObjectToChange = document;
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
            // Close
            IsCustomValueVisible = false;
        }

        public string GetUploadedTime(DateTime? utcTime)
        {
            if (utcTime.HasValue && !string.IsNullOrEmpty(user.TimeZone))
            {
                return TimeZoneInfo.ConvertTimeFromUtc(utcTime.Value, TimeZoneInfo.FindSystemTimeZoneById(user.TimeZone)).ToString();
            }
            else if (utcTime.HasValue)
            {
                return TimeZoneInfo.ConvertTimeFromUtc(utcTime.Value, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time")).ToString();
            }
            return string.Empty;
        }

        public bool SetPropertyByName(object obj, string name, object value)
        {
            var prop = obj.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            if (null == prop || !prop.CanWrite) return false;
            prop.SetValue(obj, value, null);
            return true;
        }
        #endregion
    }
}
