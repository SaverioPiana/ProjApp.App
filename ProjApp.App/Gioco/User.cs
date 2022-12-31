
using Mapsui.UI.Maui;
using NetTopologySuite.GeometriesGraph;
using ProjApp.MapEl;
using System.Reflection;
using Position = Mapsui.UI.Maui.Position;

namespace ProjApp.Gioco
{
    public class User
    {
        public string Nickname { get; set; }
        public string UserID { get; private set; }

        public Pin UserPin { get; set; }
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
