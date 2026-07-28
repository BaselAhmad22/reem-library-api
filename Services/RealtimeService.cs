using Elibrary.Api.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Elibrary.Api.Services;

public class RealtimeService
{
    private readonly IHubContext<LibraryHub> _hub;

    public RealtimeService(IHubContext<LibraryHub> hub) => _hub = hub;

    public Task PublishAsync(string entity, string action, object? data = null)
        => _hub.Clients.Group("library").SendAsync("libraryChanged", new
        {
            entity,
            action,
            data,
            at = DateTime.UtcNow
        });
}
