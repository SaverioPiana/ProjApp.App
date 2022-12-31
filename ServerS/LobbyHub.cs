using Microsoft.AspNetCore.SignalR;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace ServerS
{
    public class LobbyHub : Hub
    {

        public async Task SendPosition(string user)
        {
            await Clients.All.SendAsync("PositionReceived", user);
        }
    }
}
