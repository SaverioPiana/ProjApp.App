using Microsoft.AspNetCore.SignalR;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace ServerS
{
    public class LobbyHub : Hub
    {
        private static readonly Dictionary<string, Lobby> lobbies = new Dictionary<string, Lobby>();
        public async Task SendPosition(string user, string lobbid)
        {
            await Clients.OthersInGroup(lobbid).SendAsync("PositionReceived", user);
           

        }

        public void CreateLobby(string id)
        {
            // create a new lobby
            var lobby = new Lobby(id);

            // add the lobby to the list of lobbies
            lobbies.Add(lobby.Id, lobby);
        }

        public void JoinLobby(string lobbyId)
        {
            // find the lobby with the specified ID
            var lobby = lobbies[lobbyId];

            // add the client to the list of connected clients for the lobby
            string clientId = Context.ConnectionId;
            lobby.ConnectedClients.Add(clientId);

            Groups.AddToGroupAsync(clientId, lobbyId);

        }

        public void LeaveLobby(string lobbyId)
        {
            // find the lobby with the specified ID
            var lobby = lobbies[lobbyId];

            // remove the client from the list of connected clients for the lobby
            string clientId = Context.ConnectionId;
            lobby.ConnectedClients.Remove(clientId);

            Groups.RemoveFromGroupAsync(clientId, lobbyId);
        }

        public void StartGame(string lobbyId)
        {
            // find the lobby with the specified ID
            var lobby = lobbies[lobbyId];
            lobby.isStarted = true;
            // invoke the GameStarted clients method
            Clients.Group(lobbyId).SendAsync("GameStarted"); 

        }
    }
}
