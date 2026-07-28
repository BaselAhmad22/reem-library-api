using Microsoft.AspNetCore.SignalR;

namespace Elibrary.Api.Hubs;

public class LibraryHub : Hub
{
    public const string HubPath = "/hubs/library";

    public override async Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "library");
        await base.OnConnectedAsync();
    }
}
