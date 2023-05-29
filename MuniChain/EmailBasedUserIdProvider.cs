using Microsoft.AspNetCore.SignalR;

public class EmailBasedUserIdProvider : IUserIdProvider
{
    public virtual string GetUserId(HubConnectionContext connection)
    {
        return connection.User.Claims.FirstOrDefault(x => x.Type == "emails").Value;
    }
}