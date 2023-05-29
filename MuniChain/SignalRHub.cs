using Microsoft.AspNetCore.SignalR;

namespace UI
{
    public class SignalRHub : Hub
    {
        public const string HubUrl = "/signalRHub";

        public async Task SendMessageAsync(string senderName, string fromUserId, string[] toUserEmails, int conversationId, string message)
        {
            await Clients.Users(toUserEmails).SendAsync("ReceiveMessage", senderName, fromUserId, conversationId, message);
        }
        public async Task SendAnnotationAsync(string annotation, string[] toUserEmails, string? documentId)
        {
            await Clients.Users(toUserEmails).SendAsync("ReceiveAnnotation", annotation, toUserEmails, documentId);
        }

        public async Task SendCommentAsync(string comment, string[] toUserEmails, string? documentId)
        {
            await Clients.Users(toUserEmails).SendAsync("ReceiveAnnotationComment", comment, toUserEmails, documentId);
        }

        public async Task DealUpdated(string dealId, string? updatedBy)
        {
            await Clients.All.SendAsync("DealUpdated", dealId, updatedBy);
        }
    }
}
