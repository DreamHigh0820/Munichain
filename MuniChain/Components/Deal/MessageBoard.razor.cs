using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using UI.Components.Other;
using Shared.Helpers;
using Shared.Models.DealComponents;
using Shared.Models.Users;
using Syncfusion.Blazor.RichTextEditor;
using Hangfire;

namespace UI.Components.Deal
{
    public partial class MessageBoard
    {
        [CascadingParameter]
        public Error? Error { get; set; }
        [Parameter]
        public DealModel dealInformation { get; set; }
        [Parameter]
        public List<DealParticipant> participants { get; set; }
        private SfRichTextEditor rtfEditor = new();

        private User user;
        private bool loading = true;
        private List<BoardMessage> _messages = new List<BoardMessage>();
        private string messageToPost = "";
        private List<ToolbarItemModel> Tools = new List<ToolbarItemModel>() {
        //new ToolbarItemModel() { Name = "Save", TooltipText = "Save to Message Board"},
        //new ToolbarItemModel() { Name = "AI", TooltipText = "Save to Message Board"},
        new ToolbarItemModel() { Command = ToolbarCommand.Bold },
        new ToolbarItemModel() { Command = ToolbarCommand.Italic },
        new ToolbarItemModel() { Command = ToolbarCommand.Underline },
        new ToolbarItemModel() { Command = ToolbarCommand.StrikeThrough },
        new ToolbarItemModel() { Command = ToolbarCommand.Separator },
        new ToolbarItemModel() { Command = ToolbarCommand.FontColor },
        new ToolbarItemModel() { Command = ToolbarCommand.BackgroundColor },
        new ToolbarItemModel() { Command = ToolbarCommand.Separator },
        new ToolbarItemModel() { Command = ToolbarCommand.Formats },
        new ToolbarItemModel() { Command = ToolbarCommand.Alignments },
        new ToolbarItemModel() { Command = ToolbarCommand.Separator },
        new ToolbarItemModel() { Command = ToolbarCommand.OrderedList },
        new ToolbarItemModel() { Command = ToolbarCommand.UnorderedList },
        new ToolbarItemModel() { Command = ToolbarCommand.Separator },
        new ToolbarItemModel() { Command = ToolbarCommand.Outdent },
        new ToolbarItemModel() { Command = ToolbarCommand.Indent },
        new ToolbarItemModel() { Command = ToolbarCommand.Separator },
        new ToolbarItemModel() { Command = ToolbarCommand.CreateLink },
        new ToolbarItemModel() { Command = ToolbarCommand.CreateTable },
        new ToolbarItemModel() { Command = ToolbarCommand.Separator },
        new ToolbarItemModel() { Command = ToolbarCommand.Undo },
        new ToolbarItemModel() { Command = ToolbarCommand.Redo },
    };
        //public delegate void MyDelegate(string input, bool refreshView);
        //public MyDelegate sendUpdates;

        protected override async Task OnInitializedAsync()
        {
            loading = true;
            var authState = await authenticationStateProvider.GetAuthenticationStateAsync();
            user = await userService.GetUserById(authState.ToUser().Id);
            //sendUpdates = new MyDelegate(AIStringChanged);

            telemetry.Context.User.Id = user.Id;
            telemetry.TrackPageView("Message Board");

            _messages = new List<BoardMessage>(await messageBoardService.SearchByDealId(dealInformation.Id));
            loading = false;
        }

        /*private async Task GenerateAI()
        {
            messageToPost = "";
            await aiService.GetMessageBoard(dealInformation, participants, user, sendUpdates);
        }
        
        private void AIStringChanged(string e, bool refreshView)
        {
            if (refreshView)
            {
                rtfEditor.RefreshUIAsync();
            }
            else
            {
                messageToPost = e;
                StateHasChanged();
            }
        }*/

        private async Task PostMessage()
        {
            try
            {
                var count = await rtfEditor.GetCharCountAsync();
                if (count != 0)
                {
                    var message = new BoardMessage()
                    {
                        Message = messageToPost,
                        DateGivenUTC = DateTime.Now.ToUniversalTime(),
                        GivenByName = user.DisplayName,
                        GivenByUserId = user.Id,
                        DealId = dealInformation.Id,
                        Id = Guid.NewGuid().ToString()
                    };

                    await messageBoardService.Create(message);
                    BackgroundJobs.Enqueue(() => notificationService.MessageBoardCommentNotification(user, dealInformation));

                    _messages.Add(message);
                    _messages = _messages.OrderByDescending(x => x.DateGivenUTC).ToList();
                }

                messageToPost = "";
            }
            catch (Exception ex)
            {
                Error?.ProcessError("Failed to post message", ex, user);
                return;
            }
        }

        private async Task OnBeforeInternalNavigation(LocationChangingContext context)
        {
            var count = await rtfEditor.GetCharCountAsync();
            if (count == 0) return;

            var isConfirmed = await JsRuntime.InvokeAsync<bool>("confirm", new object[] { "Are you sure you want to navigate away from this page?" });
            if (!isConfirmed)
            {
                context.PreventNavigation();
            }
        }
    }
}
