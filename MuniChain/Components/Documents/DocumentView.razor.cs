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
using Annotation = Domain.Models.DealComponents.DocumentAnnotation;
using JsonSerializer = System.Text.Json.JsonSerializer;
using Domain.Models.DealComponents;
using ToolbarItem = Syncfusion.Blazor.PdfViewer.ToolbarItem;
using Syncfusion.Blazor.DocumentEditor;
using System.IO;
using System.ComponentModel;
using SkiaSharp;

namespace UI.Components.Documents
{
    public partial class DocumentView
    {
        [CascadingParameter]
        public Error? Error { get; set; }
        [Parameter]
        public string DocumentId { get; set; }
        [Parameter]
        public string saved { get; set; }


        public MemoryStream file;
        public WordDocument wordDocument;
        SfDocumentEditorContainer Container;
        public string sfdtString;

        public string emailAddress;
        public User user;
        public Document document;
        public List<DealParticipant> dealParticipants;
        public bool isBoldSignDocument;
        public BoldSign.Model.DocumentProperties boldSignDocumentProperties;
        public DealModel deal;
        SfPdfViewerServer Viewer;
        List<Annotation> annotations = new();
        List<Annotation> origAnnotations;
        private string message;
        private bool uploading = false;
        private bool isWordDocument;
        private User SomeoneEditing;

        public List<NotificationMarkup> notifications { get; set; } = new List<NotificationMarkup>();
        public List<Notification> events { get; set; }
        public PdfViewerToolbarSettings ToolbarSettings = new PdfViewerToolbarSettings()
        {
            ToolbarItems = new List<ToolbarItem>()
           {
                ToolbarItem.PageNavigationTool,
                ToolbarItem.MagnificationTool,
                ToolbarItem.SearchOption,
                ToolbarItem.PrintOption,
                ToolbarItem.DownloadOption,
                ToolbarItem.OpenOption,
            }
        };
        public bool loading { get; set; } = true;

        protected override async Task OnInitializedAsync()
        {
            loading = true;
            var authState = await authenticationStateProvider.GetAuthenticationStateAsync();
            user = authState.ToUser();
            telemetry.Context.User.Id = user.Id;
            telemetry.TrackPageView("Document View");
            emailAddress = user.Email;

            try
            {
                // Check document origin and deal information
                document = await documentService.GetById(DocumentId);
                //this will set up our viewable annotation list
                annotations = await annotationService.GetAllAnnotations(document?.Id, user.Id);
                origAnnotations = annotations.Clone();
            }
            catch (Exception ex)
            {
                Error?.ProcessError("Failed to get document by ID", ex, user);
                return;
            }

            var isSignatureDocument = document == null;

            // Document is coming from Boldsign
            if (isSignatureDocument)
            {
                isWordDocument = false;
                await GetSignatureDocument();
            }
            else // Document is coming from Blob storage
            {
                await GetBlobDocument();
            }

            loading = false;
            StateHasChanged();

            if (isWordDocument)
            {
                if (PageTracker.PageTracking.TryGetValue(document.Id, out var userId))
                {
                    SomeoneEditing = await userService.GetUserById(userId);
                }
                else
                {
                    PageTracker.PageTracking.Add(document.Id, user.Id);
                }
            }

            if (isWordDocument)
            {
                WordDocument document = WordDocument.Load(file, ImportFormatType.Docx);
                sfdtString = JsonSerializer.Serialize(document);
                document.Dispose();
                document = null;
                file.Dispose();
                file = null;
            }
            else
            {
                if (file == null)
                {
                    navigationManager.NavigateTo($"/404", true);
                    return;
                }
                await Viewer.LoadAsync(file);
                file.Dispose();
                file = null;
            }
            StateHasChanged();
        }

        public void OnLoad(object args)
        {
            if (!String.IsNullOrEmpty(sfdtString))
            {
                SfDocumentEditor editor = Container.DocumentEditor;
                editor.OpenAsync(sfdtString);
                //To observe the memory go down, null out the reference of sfdtString variable.
                sfdtString = null;
            }
        }

        private async Task AddAnnotation()
        {
            var newAnnotation = new Annotation()
            {
                Id = Guid.NewGuid().ToString(),
                DocumentID = document.Id,
                CreatedById = user.Id,
                IsPublished = false,
                IsEdit = true,
                Author = user.DisplayName,
                CreatedTime = DateTimeOffset.UtcNow,
                PageNumber = Viewer.CurrentPageNumber,
                TextMarkupContent = ""
            };

            await annotationService.CreateAnnotation(newAnnotation);
            annotations.Add(newAnnotation);
        }

        private async Task SaveAnnotation(DocumentAnnotation annotation)
        {
            var participants = dealParticipants.Where(x => x.UserId != user.Id).Select(x => x.EmailAddress).ToList();
            try
            {
                await annotationService.UpdateAnnotation(annotation);
                //if annotation is not published don't send alerts
                if (!annotation.IsPublished)
                {
                    return;
                }
                else if (origAnnotations.Contains(annotation))
                {
                    var diffs = annotation.ConcurrencyCompare(origAnnotations.FirstOrDefault(x => x.Id == annotation.Id));
                    if (diffs.Any())
                    {
                        //notification annotation changes
                        //await notificationService.DocAnnotationChanged(document, user, participants);
                    }
                }
                else
                {
                    //await notificationService.DocAnnotationAdded(document, user, participants);
                }

                toastService.ShowSuccess("Updated comment.");
            }
            catch (Exception ex)
            {
                Error?.ProcessError("Failed to save annotation", ex, user, null);
                return;
            }

            JsonSerializerOptions options = new()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
            annotation.IsEdit = false;
        }

        private async Task GetBlobDocument()
        {
            isBoldSignDocument = false;
            bool hasBeenPublished;
            try
            {
                dealParticipants = await dealParticipantService.GetParticipantsByDealId(document?.DealId);
                deal = await dealService.GetById(document?.DealId, DealViewType.ByID);
                hasBeenPublished = await dealService.HasBeenPublished(deal.Id, deal.HistoryDealID);
            }
            catch (Exception ex)
            {
                Error?.ProcessError("Failed to get document from Blob storage", ex, user, deal);
                return;
            }

            // implement public view for document
            if (DocumentPermissionValidator.Validate(document, dealParticipants, user, false, hasBeenPublished))
            {
                // Insert Event on Open if document not saved
                if (saved != "saved" && user.Email != document.CreatedBy)
                {
                    await notificationService.Create(new Notification(document, user, deal, document.CreatedBy)
                    {
                        Action = NotificationAction.Opened
                    });
                }

                try
                {
                    // Load Document
                    var resp = await fileService.GetFile(DocumentId, deal.Id);
                    isWordDocument = (resp.Item2 == DocumentType.Pdf || resp.Item2 == DocumentType.Powerpoint)
                                                ? false : true;

                    file = (MemoryStream)resp.Item1;

                }
                catch (Exception ex)
                {
                    Error?.ProcessError("Failed to get file from file service", ex, user, deal);
                    return;
                }

                // We maintain document history/audit in Munichain database
                events = await notificationService.Search(x => x.DocumentId == DocumentId);
                events.ForEach(x => notifications.Add(x.ToNotification()));
                notifications = notifications.Where(x => x != null).ToList();
            }
        }
        private async Task GetSignatureDocument()
        {
            try
            {
                isBoldSignDocument = true;
                var boldsignDocument = await boldsignService.GetDocument(user.Email, DocumentId);
                if (boldsignDocument == null)
                {
                    navigationManager.NavigateTo($"/404", true);
                    return;
                }

                var doc = boldsignDocument.First();
                boldSignDocumentProperties = doc.Key;
                deal = await dealService.GetById(boldsignDocument?.First().Key?.Labels.First(), DealViewType.ByID);
                file = doc.Value;
            }
            catch (Exception ex)
            {
                Error?.ProcessError("Failed to get document from Boldsign", ex, user);
                loading = false;
                return;
            }
        }

        private async Task GoToAnnotation(int pageNumber)
        {
            await Viewer.GoToPageAsync(pageNumber + 1);
        }

        private async Task DeleteAnnotation(Annotation annotation)
        {
            try
            {
                //should not be able to delete published annotations
                if (!annotation.IsPublished)
                {
                    await Viewer.SelectAnnotationAsync(annotation.Id);
                    await Viewer.DeleteAnnotationAsync();
                    await annotationService.DeleteAnnotation(annotation);
                    annotations.Remove(annotation);
                }
                else
                {
                    toastService.ShowError("Cannot delete published annotations");
                    return;
                }

            }
            catch (Exception ex)
            {
                Error?.ProcessError("Failed to delete annotation", ex, user, null);
                return;
            }
        }
        private async Task PublishAnnotation(Annotation annotation)
        {
            JsonSerializerOptions options = new()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
            var participants = dealParticipants.Where(x => x.UserId != user.Id).Select(x => x.EmailAddress).ToArray();

            annotation.IsPublished = true;
            await annotationService.UpdateAnnotation(annotation);
            var json = JsonSerializer.Serialize(annotation, options);
        }

        private async Task AddComment(Annotation annotation)
        {
            var newComment = new AnnotationComment()
            {
                Id = Guid.NewGuid().ToString(),
                CreatedById = user.Id,
                CreatedByUserName = user.DisplayName,
                DocumentAnnotationId = annotation.Id,
                Message = message,
                CreatedTime = DateTimeOffset.UtcNow,
                IsDeleted = false,
                IsEdit = true
            };

            annotation.Comments.Add(newComment);
        }

        private async Task SaveComment(AnnotationComment comment, Annotation annotation)
        {
            var participants = dealParticipants.Where(x => x.UserId != user.Id).Select(x => x.EmailAddress).ToList();
            try
            {
                await annotationService.CreateComment(comment, annotation, user);

                //if annotation is not published don't send alerts
                if (!annotation.IsPublished)
                {
                    return;
                }
                else if (origAnnotations.Contains(annotation))
                {
                    var diffs = annotation.ConcurrencyCompare(origAnnotations.FirstOrDefault(x => x.Id == annotation.Id));
                    if (diffs.Any())
                    {
                        //notification annotation changes
                        //await notificationService.DocAnnotationChanged(document, user, participants);
                        JsonSerializerOptions options = new()
                        {
                            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                        };
                        var json = JsonSerializer.Serialize(comment, options);
                    }
                }
                else
                {
                    //notification new annotation
                    //await notificationService.DocAnnotationCommentAdded(document, user, participants, annotation.Id);
                    JsonSerializerOptions options = new()
                    {
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                    };
                    var json = JsonSerializer.Serialize(comment, options);
                }


            }
            catch (Exception ex)
            {
                Error?.ProcessError("Failed to add comment.", ex, user, null);
                return;
            }

            comment.IsEdit = false;
        }

        public async Task OnSaveDocument()
        {
            uploading = true;
            await InvokeAsync(StateHasChanged);
            var fileId = document.Id;
            string base64Data = await Container.DocumentEditor.SaveAsBlobAsync(FormatType.Docx);
            byte[] data = Convert.FromBase64String(base64Data);

            try
            {

                await documentService.Update(document);
                await fileService.UploadFile(data, "word", document.DealId, fileId);
                toastService.ShowSuccess("Saved file successfully.");
                uploading = false;
            }
            catch (Exception e)
            {

            }
        }

        private async Task DeleteComment(Annotation annotation, AnnotationComment annotationComment)
        {
            annotationComment.IsDeleted = true;
            try
            {
                await annotationService.UpdateAnnotation(annotation);
                StateHasChanged();
            }
            catch (Exception ex)
            {
                Error?.ProcessError("Failed to delete annotation comment", ex, user, null);
                return;
            }
            return;
        }

        public void Dispose()
        {
            if (PageTracker.PageTracking.Any() && PageTracker.PageTracking.TryGetValue(document?.Id, out string userId) && userId == user?.Id)
            {
                PageTracker.PageTracking.Remove(document?.Id);
            }
        }
    }
}
