using Microsoft.AspNetCore.SignalR;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServerS
{
    public class LobbyHub : Hub
    {
        private static Dictionary<string, Lobby> lobbies = new Dictionary<string, Lobby>();
        public async Task SendPosition(string user, string lobbid)
        {
            //await Clients.OthersInGroup(lobbid).SendAsync("PositionReceived", user);
            await Clients.Group(lobbid).SendAsync("PositionReceived", user);


        }

        public void CreateLobby(string id)
        {
            // create a new lobby
            var lobby = new Lobby(id);

            // add the lobby to the list of lobbies
            lobbies.Add(lobby.Id, lobby);
        }

        public async Task JoinLobby(string lobbyId)
        {
            // find the lobby with the specified ID
            var lobby = lobbies[lobbyId];

            // add the client to the list of connected clients for the lobby
            string clientId = Context.ConnectionId;
            lobby.ConnectedClients.Add(clientId);

            await Groups.AddToGroupAsync(clientId, lobbyId);

            string mess = $"{clientId} joined {lobbyId}";

            await Clients.All.SendAsync("ServerMessage", mess);

        }

        public async Task LeaveLobby(string lobbyId)
        {
            // find the lobby with the specified ID
            var lobby = lobbies[lobbyId];

            // remove the client from the list of connected clients for the lobby
            string clientId = Context.ConnectionId;
            lobby.ConnectedClients.Remove(clientId);

            await Groups.RemoveFromGroupAsync(clientId, lobbyId);
        }

        public async Task StartGame(string lobbyId)
        {
            // find the lobby with the specified ID
            var lobby = lobbies[lobbyId];
            lobby.isStarted = true;
            foreach (string s in lobby.ConnectedClients){
                string mess = $"Giocatore :({s}) nella lobby : ({lobbyId})";
                await Clients.All.SendAsync("ServerMessage", mess);
            }
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
                    await Clients.Client(cacciatore).SendAsync("GameStarted" , true);
                }
            }

            // invoke the GameStarted clients method
            await Clients.GroupExcept(lobbyId, lobby.cacciatori).SendAsync("GameStarted" , false);
            //await Clients.Group(lobbyId).SendAsync("GameStarted", false);

        }
    }
}
