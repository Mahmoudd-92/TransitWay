using Microsoft.AspNetCore.SignalR;

namespace TransitWay.Hubs
{
    public class TrackingHub : Hub
    {
        public async Task SendLocationUpdate(string id, double lat, double lng)
        {
            await Clients.All.SendAsync("ReceiveLocationUpdate", id, lat, lng);
        }
    }
}