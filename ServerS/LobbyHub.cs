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
            int num_clients = lobby.ConnectedClients.Count();
            int num_cacciatori = num_clients / 4;
            Random random = new Random();
            if (num_cacciatori == 0)
            {
                lobby.cacciatori.Add(lobby.ConnectedClients[random.Next(num_clients - 1)]);
            }
            else
            {
                for (int i = 0; i < num_cacciatori; i++)
                {
                    string cacciatore = lobby.ConnectedClients[random.Next(num_clients - 1)];
                    lobby.cacciatori.Add(cacciatore);
                    Clients.Client(cacciatore).SendAsync("GameStartedAsCacciatore");
                }
            }

            // invoke the GameStarted clients method
            Clients.GroupExcept(lobbyId, lobby.cacciatori).SendAsync("GameStarted");

        }
    }
}
