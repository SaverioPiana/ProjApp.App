using Mapsui.Extensions;
using Mapsui.UI.Maui;
using ProjApp.Map.GPS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ProjApp.Gioco
{
    public class User
    {
        public string Nickname {  get; private set; }
        public string UserID { get; private set; }
        public Position position
        {
            get { return position; }

            set 
            {
                if (position != value)
                {
                    position = value;
                    this.pin.Position = position;
                }
            }
        }

        //CRASHA
        ////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        public Pin pin;
        byte[] UserIcon = Assembly.GetExecutingAssembly().GetManifestResourceStream
                            (Assembly.GetExecutingAssembly().GetManifestResourceNames().Single(
                                str => str.EndsWith("pinicon.png"))).ToBytes();
        //////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////
        public User(string nickname, string userID, Position posizione)
        {
            Nickname = nickname;
            UserID = userID;
            this.position = posizione;
            this.pin = new Pin
            {
                Icon = UserIcon,
                Label = UserID,
                Position = posizione
            };
        }
    }
}
