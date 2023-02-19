using Microsoft.AspNetCore.SignalR;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServerS
{
    public class LobbyHub : Hub
    {
        //messaggi di errore id lobby
        private const string PARTITA_IN_CORSO = "La partita e' gia' iniziata";
        private const string INVALID_ID = "Pare proprio non esista una partita con quell'ID";
        private const string ADMIN_LEFT = "L'admin ha chiuso baracca, entra in un'altra lobby";
        private const string GENERIC_ERROR = "Boh riapri lapp, errore de cristo";

        public static Dictionary<string, Lobby> lobbies = new Dictionary<string, Lobby>();
        public async Task SendPosition(string user, string lobbid)
        {
            //await Clients.Caller.SendAsync("ServerMessage", "ryewuirywu");
            await Clients.OthersInGroup(lobbid).SendAsync("PositionReceived", user);
            //await Clients.Group(lobbid).SendAsync("PositionReceived", user);
        }
        
        public void CreateLobby(string id)
        {
            //Clients.Caller.SendAsync("ServerMessage", id);
            // create a new lobby
            var lobby = new Lobby(id);
            //Clients.Caller.SendAsync("ServerMessage", lobby!=null ? lobby.ToString() : "lobby nulla");

            // add the lobby to the list of lobbies
            lobbies.Add(lobby.Id, lobby);
            //Clients.Caller.SendAsync("ServerMessage", "added" + lobby.ToString());
        }

        public async Task IfCheckThenJoin(string lobbyId, string jsonUser)
        {
            try 
            {
                //await Clients.Caller.SendAsync("ServerMessage", "ryewuirywu");
                //await Clients.Caller.SendAsync("ServerMessage", lobbyId);
                bool lobbyvalid = lobbies.ContainsKey(lobbyId);
                if (lobbyvalid) {
                    if (lobbies[lobbyId].IsStarted)
                    {
                        await Clients.Caller.SendAsync("ServerError", PARTITA_IN_CORSO);
                    }
                    else await Clients.Caller.SendAsync("JoinLobby", lobbyId, jsonUser);
                }
                else {
                    await Clients.Caller.SendAsync("ServerError", INVALID_ID); }
            } catch(Exception e)
            {
                await Clients.Caller.SendAsync("ServerError", GENERIC_ERROR );
            }
        }

        public async Task JoinLobby(string lobbyId, string jsonUser)
        {
            // find the lobby with the specified ID
            var lobby = lobbies[lobbyId];
            // add the client to the list of connected clients for the lobby
            string clientId = Context.ConnectionId;
            lobby.ConnectedClients.Add(clientId);
            await Groups.AddToGroupAsync(clientId, lobbyId);

            //string mess = $"{clientId} joined {lobbyId}";
            //await Clients.Caller.SendAsync("ServerMessage", "SEI TU --->");
            //await Clients.Group(lobbyId).SendAsync("ServerMessage", mess);
            //se ci sei solo tu non serve
            if (lobby.ConnectedClients.Count > 1)
            {
                await Clients.OthersInGroup(lobbyId).SendAsync("AddUserFromServer", jsonUser, clientId, true);
            }
        }

        public async Task SendToLastJoined(string clientId, string jsonToSend)
        {
            await Clients.Client(clientId).SendAsync("AddUserFromServer", jsonToSend, clientId, false);
        }


        public async Task InviaOggettiDiGioco(string lobbyId, string area , string tana)
        {
            await Clients.OthersInGroup(lobbyId).SendAsync("RiceviOggettiDiGioco",
                arg1: area,
                arg2: tana);
            //await Clients.Group(lobbyId).SendAsync("ServerMessage", "OGGETTI DI GIOCO INVIATI");

        } 
        public async Task LeaveLobby(string lobbyId, string uid)
        {

            // find the lobby with the specified ID
            var lobby = lobbies[lobbyId];

            // remove the client from the list of connected clients for the lobby
            string clientId = Context.ConnectionId;
            lobby.ConnectedClients.Remove(clientId);
            await Groups.RemoveFromGroupAsync(clientId, lobbyId);
            await Clients.Group(lobbyId).SendAsync("UserLeft", uid);
        }

        public async Task DeleteLobby(string lobbyId)
        {
            // find the lobby with the specified ID
            var lobby = lobbies[lobbyId];
            //elimino la lobby
            lobbies.Remove(lobbyId);
            await Clients.OthersInGroup(lobbyId).SendAsync("DeletedLobby");
            await Clients.OthersInGroup(lobbyId).SendAsync("ServerError", ADMIN_LEFT);
        }

        public async Task StartGame(string lobbyId)
        {
            // find the lobby with the specified ID
            var lobby = lobbies[lobbyId];
            lobby.IsStarted = true;
            //foreach (string s in lobby.ConnectedClients){
              //  string mess = $"Giocatore :({s}) nella lobby : ({lobbyId})";
               // await Clients.Group(lobbyId).SendAsync("ServerMessage", mess);
            //}
            int num_clients = lobby.ConnectedClients.Count();
            int num_cacciatori = num_clients / 3;
            Random random = new Random();
            if (num_cacciatori == 0)
            {
                string cacciatore = lobby.ConnectedClients[random.Next(num_clients)];
                lobby.cacciatori.Add(cacciatore);
                await Clients.Client(cacciatore).SendAsync("GameStarted", true);
            }
            else
            {
                for (int i = 0; i < num_cacciatori; i++)
                {
                    string cacciatore = lobby.ConnectedClients[random.Next(num_clients)];
                    lobby.cacciatori.Add(cacciatore);
                    await Clients.Client(cacciatore).SendAsync("GameStarted" , true);
                }
            }
            // invoke the GameStarted clients method
            await Clients.GroupExcept(lobbyId, lobby.cacciatori).SendAsync("GameStarted" , false);
            //await Clients.Group(lobbyId).SendAsync("GameStarted", false);
        }

        public async Task GiocatorePreso(string lid)
        {
            var lobby = lobbies[lid];
            lobby.NumGiocatoriPresi++;
            if (lobby.IsPartitaFinita())
            {
                await Clients.Group(lid).SendAsync("FinePartita");
            }
        }

        public async Task GiocatoreTanato(string lid)
        {
            var lobby = lobbies[lid];
            lobby.NumGiocatoriTanati++;
            if (lobby.IsPartitaFinita())
            {
                await Clients.Group(lid).SendAsync("FinePartita");
            }
        }
    }
}
