
using Mapsui.UI.Maui;
using Microsoft.AspNetCore.SignalR.Client;
using NetTopologySuite.GeometriesGraph;
using ProjApp.MapEl;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Text.Json;
using Position = Mapsui.UI.Maui.Position;
using Java.Util;
using Random = System.Random;
using NetTopologySuite.Triangulate;
using System.Globalization;
using Java.Lang;

namespace ProjApp.Gioco
{
    public class User
    {
        ////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////IMPORTANTE/////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////

        //LE ICONE DELL UTENTE DA ORA IN POI DEVONO PER FORZA FINIRE PER "pinicon.png" O
        // NON LE CARICA, BELLA

        ////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////IMPORTANTE/////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////
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

        [JsonIgnore]
        private static List<string> icone = GetIconsFromImages();

        [JsonInclude]
        public byte[] UserIcon = OurMapController.ReadResource(icone.ToArray()[new Random().Next(0, 3)]);

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
                Scale = 0.4F
            };
            position = posizione;
        }

        private static List<string> GetIconsFromImages()
        {
            List<string> icons = new();
            foreach(string resource in Assembly.GetExecutingAssembly().GetManifestResourceNames())
            {
                if (resource.EndsWith("pinicon.png")) {
                    
                    List<string> path = resource.Split(".").ToList();
                    StringBuilder filename = new();
                    foreach(string s in path)
                    {
                        if(s.EndsWith("pinicon"))
                        {
                            //non è supportato per ios????
                            filename.Append(s);
                            filename.Append(".");
                        }
                        if (s.EndsWith("png")) filename.Append(s);
                    }
                    icons.Add(filename.ToString());
                }
            }
            return icons;
        }
    }
}
