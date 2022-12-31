
using Mapsui.UI.Maui;
using Microsoft.AspNetCore.SignalR.Client;
using NetTopologySuite.GeometriesGraph;
using ProjApp.MapEl;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Text.Json;
using Position = Mapsui.UI.Maui.Position;

namespace ProjApp.Gioco
{
    public class User
    {
        public string Nickname { get; set; }
        public string UserID { get; set; }
        public Pin UserPin { get; set; }
        [JsonIgnore]
        public byte[] UserIcon = OurMapController.ReadResource(Assembly.GetExecutingAssembly(), "pinicon.png");

        public User(string nickname, string userID, Position posizione, MapView mv)
        {
            Nickname = nickname;
            UserID = userID;
            UserPin = new Pin(mv)
            {
                Label = userID,
                Position = posizione,
                Type = PinType.Icon,
                Icon = UserIcon,
                Scale = 0.6F
            };
        }
    }
}
