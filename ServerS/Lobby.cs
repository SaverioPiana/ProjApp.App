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

        //va aggiornata con i gioactori crashati (non ho contatti da loro per tipo 30 se / 1 min)
        public List<string> ConnectedClients { get; set; } = new List<string>();
        //public int NumGiocatoriPresi { get; set; } = 0;
        //public int NumGiocatoriTanati { get; set; } = 0;
        
        //public bool IsPartitaFinita()
        //{
        //    return ((NumGiocatoriPresi + NumGiocatoriTanati) == (ConnectedClients.Count - cacciatori.Count));
        //}

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
