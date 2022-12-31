
using Mapsui.UI.Maui;
using ProjApp.MapEl;
using System.Reflection;

namespace ProjApp.Gioco
{
    public class User
    {
        public string Nickname { get; private set; }
        public string UserID { get; private set; }
        public Position position { get; set; }
        public Pin UserPin { get; private set; }
        public byte[] UserIcon = OurMapController.ReadResource(Assembly.GetExecutingAssembly(),"pinicon.png");

        public User(string nickname, string userID, Position posizione, MapView mv)
        {
            Nickname = nickname;
            UserID = userID;
            position = posizione;
            UserPin = new Pin(mv)
            {
                Label = userID,
                Position = position,
                Type = PinType.Icon,
                Icon = UserIcon,
                Scale = 0.6F
            };
        }
    }
}
