using Microsoft.AspNetCore.SignalR;

namespace KR5.Hubs;

public class ProductionHub : Hub
{
    public async Task UpdateProgress(int orderId, int progress)
    {
        await Clients.All.SendAsync("ReceiveProgress", orderId, progress);
    }
}
