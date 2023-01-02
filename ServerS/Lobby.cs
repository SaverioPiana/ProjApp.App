namespace ServerS
{
    public class Lobby
    {
        public string Id { get; set; }
        public bool isStarted { get; set; }
        public List<string> ConnectedClients { get; set; } = new List<string>();



        public Lobby(string id)
        {
            Id = id;
        }   
    }

}
