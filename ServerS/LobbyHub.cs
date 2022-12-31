using Microsoft.AspNetCore.SignalR;

namespace ServerS
{
    public class LobbyHub : Hub
    {
        public async Task SendPosition(object user)
        {
            await Clients.All.SendAsync("PositionReceived", user);
        }
    }
}
