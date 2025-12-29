using Microsoft.AspNetCore.SignalR;

namespace Order.Service.Hubs
{
    public class OrderHub : Hub
    {
        // Clients (browser) will call this to subscribe to a specific order's updates
        public async Task JoinOrderGroup(string orderId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, orderId);
            // Helpful for debugging in the console
            Console.WriteLine($"[SignalR] Client {Context.ConnectionId} joined Group: {orderId}");
        }
    }
}