using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Comedor.Infrastructure.Hubs;

public class DispatchHub : Hub
{
    // Methods can be added here for client-to-server communication if needed.
    // For now, it's primarily used for server-to-client notifications.
    public async Task SendMessage(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }
}
