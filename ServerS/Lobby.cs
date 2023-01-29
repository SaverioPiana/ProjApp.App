using Microsoft.IdentityModel.Tokens;

namespace ServerS
{
    public class Lobby
    {
        private const long TicksPerHour = 36000000000;
        private const int HtoMs = 3600000;
        private DateTime lastTime;
        public string Id { get; set; }
        public bool isStarted { get; set; }

        public List<string> cacciatori { get; set; } = new List<string>();
        public List<string> ConnectedClients { get; set; } = new List<string>();



        public Lobby(string id)
        {
            lastTime = DateTime.Now;
            Id = id;
            this.isExpired();
        }
        //lobbies expiring in 10h
        private async void isExpired()
        {
            while((DateTime.Now.Ticks - this.lastTime) < TicksPerHour * 10)
            {
                Task.Delay(HtoMs * 5)
            }
            LobbyHub.lobbies.Remove(this.Id);  
            LobbyHub.Se
        }
    }

}
