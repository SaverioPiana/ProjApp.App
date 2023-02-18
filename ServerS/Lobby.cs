using Microsoft.IdentityModel.Tokens;

namespace ServerS
{
    public class Lobby
    {
        private const long TicksPerHour = 36000000000;
        private const int HtoMs = 3600000;
        private DateTime lastTime;
        private bool isStarted = false;
        public string Id { get; set; }
        public bool IsStarted { get; set; }

        public List<string> cacciatori { get; set; } = new List<string>();
        public List<string> ConnectedClients { get; set; } = new List<string>();


        public Lobby(string id)
        {
            lastTime = DateTime.Now;
            Id = id;
            Task.Run(IsExpired);
        }
        //lobbies expiring in 10h
        private async void IsExpired()
        {
            while((DateTime.Now.Ticks - this.lastTime.Ticks) < TicksPerHour * 10)
            {
                await Task.Delay(HtoMs * 5);
            }
            try
            {
                LobbyHub.lobbies.Remove(this.Id);
            }
            catch (Exception ex) { }
        }
    }

}
