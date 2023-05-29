using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using Shared.Helpers;
using Shared.Models.Chat;
using Shared.Models.Users;
using Syncfusion.Blazor.Data;
using Syncfusion.Blazor.DropDowns;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace UI.Components.Chat
{
    public partial class Chat
    {
        public Query LocalDataQuery = new Query().Take(5);
        private User user;
        ObservableCollection<List<Message>> messageGroups = new ObservableCollection<List<Message>>();
        public string TypedMessage { get; set; } = "";
        SfMultiSelect<string[], User> MultiSelectUsers;
        private bool ShowAddConversation { get; set; } = false;
        private bool loading { get; set; } = false;
        private List<User> Recipients { get; set; } = new List<User>() { };

        public List<User> UserList = new();
        public List<Conversation> ConversationsList = new();
        public Conversation CurrentConversation { get; set; } = new Conversation() { Id = -1 };

        public class Message
        {
            public string FromUserId { get; set; }
            public string UserName { get; set; }
            public string MessageText { get; set; }
            public string Chat { get; set; }
        }

        [CascadingParameter] public HubConnection hubConnection { get; set; }
        [CascadingParameter] public bool ChatMinimized { get; set; }
        [Parameter] public Action OnChatChange { get; set; }

        private void MinimizeChat()
        {
            OnChatChange?.Invoke();
        }

        public async ValueTask DisposeAsync()
        {
            if (hubConnection is not null)
            {
                await hubConnection.DisposeAsync();
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            _jsRuntime.InvokeAsync<string>("ScrollToBottom", "list");
        }

        protected override async Task OnInitializedAsync()
        {
            var state = await authenticationStateProvider.GetAuthenticationStateAsync();
            user = state.ToUser();

            UserList = await userService.GetAllUsers();
            UserList.Remove(UserList.Find(u => u.Email == user.Email)); // Remove the logged in user from the list of users

            ConversationsList = await conversationService.GetConversations(user.Email);
            if (ConversationsList.Count() > 0)
            {
                CurrentConversation = ConversationsList.FirstOrDefault();
                await RetrieveConversation(CurrentConversation.Id);
            }

            hubConnection.On<string, string, int, string>("ReceiveMessage", async (senderName, fromUser, conversationId, message) =>
            {
                if (CurrentConversation.Id == conversationId && !ChatMinimized)
                {
                    Message m = new Message()
                    {
                        FromUserId = fromUser,
                        MessageText = message,
                        Chat = user.Id == fromUser ? "sender" : "receive",
                        UserName = user.Id == fromUser ? "You" : senderName
                    };
                    AddMessageToMessageGroup(m);

                    SortConversationByActivity(conversationId);
                }
                else
                {
                    Conversation conversation = ConversationsList.FirstOrDefault(c => c.Id == conversationId);

                    if (conversation != null)
                    {
                        // mark the conversation as unread
                        conversation.ConversationReadByMembers = conversation.ConversationReadByMembers.Replace(user.Email, "");
                        await conversationService.UpdateConversationReadStatus(conversationId, conversation.ConversationReadByMembers);

                        SortConversationByActivity(conversationId);
                    }
                    else // someone has started a new conversation
                    {
                        ConversationsList = await conversationService.GetConversations(user.Email);
                    }
                }

                StateHasChanged();
            });
        }

        public void SortConversationByActivity(int activeConversationId)
        {
            ConversationsList.Sort((a, b) =>
            {
                if (a.Id == activeConversationId) return -1;
                else return 0;
            });
            StateHasChanged();
        }

        public async Task Send()
        {
            if (TypedMessage != "")
            {
                var senderName = user.DisplayName;
                var fromUserId = user.Id;
                string[] toUserEmails = CurrentConversation.MemberEmails.Split(',');
                var conversationId = CurrentConversation.Id;
                var message = TypedMessage;

                if (CurrentConversation.Id == 0) // It is a new conversation
                {
                    conversationId = await conversationService.Create(CurrentConversation);
                    //await emailService.ConversationStartedNotification(senderName, toUserEmails.Where(x => x != user.Email).ToArray());
                    // Send notifications to users that chat has been opened
                }

                TypedMessage = "";
                ChatMessage messageObject = new ChatMessage
                {
                    FromUserId = fromUserId,
                    FromUserDisplayName = senderName,
                    Message = message,
                    ToConversationId = conversationId,
                    DateSentUTC = DateTime.Now.ToUniversalTime()
                };

                //bring the conversation to the top
                //SortConversationByActivity(CurrentConversation.Id);

                //send the message
                await hubConnection.SendAsync("SendMessageAsync", senderName, fromUserId, toUserEmails, conversationId, message);
                //Save message to database
                await chatService.Create(messageObject);

                //StateHasChanged();
            }
        }

        public async Task AddNewConversation()
        {
            // Remove previous unsaved new conversation
            ConversationsList.RemoveAll(c => c.Id == 0);

            // See if a conversation between the same members already exists
            foreach (var c in ConversationsList)
            {
                HashSet<string> oldConversationMembers = c.MemberEmails.Split(',').ToHashSet();
                HashSet<string> newConversationMembers = Recipients.Select(r => r.Email).Append(user.Email).ToHashSet();

                if (oldConversationMembers.SetEquals(newConversationMembers))
                {
                    ShowAddConversation = false;
                    await MultiSelectUsers.ClearAsync();
                    SwitchConversation(c);  // open the conversation that already exists
                    StateHasChanged();
                    return;
                }
            };

            // Create a new conversation object and add the members to it
            Conversation conversation = new Conversation() { DateCreatedUTC = DateTime.Now.ToUniversalTime() };
            Recipients.ForEach((r) =>
            {
                conversation.MemberIds = conversation.MemberIds + r.Id + ',';
                conversation.MemberEmails = conversation.MemberEmails + r.Email + ',';
                conversation.MemberDisplayNames = conversation.MemberDisplayNames + r.DisplayName + ", ";
            });
            conversation.MemberIds = conversation.MemberIds + user.Id;
            conversation.MemberEmails = conversation.MemberEmails + user.Email;
            conversation.MemberDisplayNames = conversation.MemberDisplayNames + user.DisplayName;
            conversation.ConversationReadByMembers = conversation.ConversationReadByMembers + user.Email;

            ShowAddConversation = false;
            await MultiSelectUsers.ClearAsync();

            ConversationsList.Add(conversation);
            SwitchConversation(conversation);
            StateHasChanged();
        }

        public async Task RetrieveConversation(int conversationId)
        {
            if (conversationId == 0)
            {
                messageGroups.Clear();
                return;
            }

            // retrieve the messages from the database
            var messagesFromDatabase = await chatService.GetMessages(conversationId);

            // Update the read status of this conversation for this user
            if (!CurrentConversation.ConversationReadByMembers.Contains(user.Email))
            {
                CurrentConversation.ConversationReadByMembers = CurrentConversation.ConversationReadByMembers + user.Email;
                await conversationService.UpdateConversationReadStatus(CurrentConversation.Id, CurrentConversation.ConversationReadByMembers);
            }

            messageGroups.Clear();
            foreach (var msg in messagesFromDatabase)
            {
                Message message = new Message
                {
                    FromUserId = msg.FromUserId,
                    Chat = user.Id == msg.FromUserId ? "sender" : "receive",
                    UserName = user.Id == msg.FromUserId ? "You" : msg.FromUserDisplayName,
                    MessageText = msg.Message
                };
                AddMessageToMessageGroup(message);
            }
            StateHasChanged();
        }

        private void AddMessageToMessageGroup(Message message)
        {
            if (messageGroups.Count() == 0)
            { // add the first message to the conversation
                messageGroups = new ObservableCollection<List<Message>>();
                messageGroups.Add(new List<Message>() { message });
                return;
            }

            if (messageGroups.Last().Last().FromUserId == message.FromUserId) // the last message and current message are from the same user
            {
                messageGroups.Last().Add(message);
            }
            else
            {
                messageGroups.Add(new List<Message>() { message });
            }
        }

        public async Task TriggerSendMessage()
        {
            SendingMessage = true;
            await Send();
            SendingMessage = false;
        }

        public async Task KeyPressed(KeyboardEventArgs args)
        {
            if (args.Key == "Enter")
            {
                await TriggerSendMessage();
            }
        }

        private void OnValueSelecthandler(SelectEventArgs<User> args)
        {
            Recipients.Add(args.ItemData);
        }
        private void OnValueRemoveHandler(RemoveEventArgs<User> args)
        {
            Recipients.Remove(args.ItemData);
        }
        private void Clearedhandler(MouseEventArgs args)
        {
            Recipients = new List<User>();
        }

        public async Task SwitchConversation(Conversation conversation)
        {
            loading = true;
            Conversation previousConversation = CurrentConversation;
            CurrentConversation = conversation;
            StateHasChanged();

            if (CurrentConversation.Id != previousConversation.Id)
                await RetrieveConversation(conversation.Id);

            loading = false;
            StateHasChanged();
        }

        public string RemoveCurrentUserName(string displayNames, string nameToBeRemoved)
        {
            string midPattern = ", " + nameToBeRemoved + ", ";
            string startPattern = "^" + nameToBeRemoved + ", ";
            string endPattern = ", " + nameToBeRemoved + "$";

            if (Regex.IsMatch(displayNames, midPattern))
            {
                displayNames = new Regex(midPattern).Replace(displayNames, ", ", 1);
            }
            else if (Regex.IsMatch(displayNames, startPattern))
            {
                displayNames = new Regex(startPattern).Replace(displayNames, "", 1);
            }
            else
            {
                displayNames = new Regex(endPattern).Replace(displayNames, "", 1);
            }

            return displayNames;
        }

        public bool IsConnected => hubConnection.State == HubConnectionState.Connected;
        public bool SendingMessage { get; set; }
    }
}
