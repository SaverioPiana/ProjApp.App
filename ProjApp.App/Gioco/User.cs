using Mapsui.UI.Maui;
using System.Reflection;
using System.Text.Json.Serialization;
using Position = Mapsui.UI.Maui.Position;
using Random = System.Random;
using System.Text;
using ProjApp.ViewModel;

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
        [JsonIgnore]
        private object lockObjectpreso = new object();
        private Location position;
        private bool isPreso;

        public IList<string> NicknameGiocatoriPresi { get; set; } = new List<string>();

        public bool IsCercatore { get; set; } = false;
        public bool IsSalvo { get; set; } = false;
        public bool IsPreso
        {
            get
            {
                lock (lockObjectpreso)
                {
                    return isPreso;
                }
            }
            set
            {
                lock (lockObjectpreso)
                {
                    isPreso = value;
                }
            }
        }
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
        public byte[] UserIcon = MainPageViewModel.ReadResource(icone.ToArray()[new Random().Next(0, 3)]);

        public User(string nickname, string userID, Location posizione)
        {
            Nickname = nickname;
            UserID = userID;
            //dobbiamo crearlo dopo perche non abbiamo piu la mapview
            UserPin = new();
            position = posizione;
            IsPreso = false;
        }

        public static List<string> GetIconsFromImages()
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
