
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
        [JsonIgnore]
        private object lockObject = new object();
        private Location position;
        private bool isCercatore = false;

        public bool IsCercatore { get; set; }
        public string Nickname { get; set; }
        public string UserID { get; set; }
        [JsonIgnore]
        public Pin UserPin { get; set; }
        public Location Position
        {
            get
            {
                lock (lockObject)
                {
                    return position;
                }
            }
            set
            {
                lock (lockObject)
                {
                    position = value;
                    UserPin.Position =
                        new Position(position.Latitude,
                                     position.Longitude);
                }
            }
        }
        [JsonInclude]
        public byte[] UserIcon = OurMapController.ReadResource(Assembly.GetExecutingAssembly(), "pinicon.png");

        public User(string nickname, string userID, Location posizione, MapView mv)
        {
            Nickname = nickname;
            UserID = userID;
            UserPin = new Pin(mv)
            {
                Label = userID,
                Position = new Position(posizione.Latitude, posizione.Longitude),
                Type = PinType.Icon,
                Icon = UserIcon,
                Scale = 0.6F
            };
            position = posizione;
        }
    }
}
