using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.SignalR;

namespace ServerS
{
    public class LobbyHub : Hub
    {
        public async Task SendPosition(string user , double lat, double lon)
        {
            await Clients.All.SendAsync("PositionReceived", user, lat, lon);
        }
    }
}
