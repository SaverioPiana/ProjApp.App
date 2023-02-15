using System.Reflection;
using BruTile.Predefined;
using BruTile.Web;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Mapsui.Projections;
using Mapsui.Tiling.Layers;
using Mapsui;
using Mapsui.UI.Maui;
using ProjApp.MapEl.GPS;
using static ProjApp.ViewModel.StartPageViewModel;
using Mapsui.UI.Maui.Extensions;
using Microsoft.AspNetCore.SignalR.Client;
using Position = Mapsui.UI.Maui.Position;
using ProjApp.Gioco;
using System.Text.Json;
using CommunityToolkit.Mvvm.Messaging.Messages;
using System.Text.Json.Serialization;
using ProjApp.MapEl.Serializable;
using System.Text;

namespace ProjApp.ViewModel
{
    public partial class MainPageViewModel : ObservableObject
    {
        [ObservableProperty]
        private MapView mapview;

        private static bool FIRST_CREATION = true;
        private static List<IDisposable> serverRegistrations = new();


        private static object lockPreMatchPins = new object();
        private static IList<Pin> preMatchPins = new List<Pin>();

        public static IList<Pin> PreMatchPins
        {
            get
            {
                lock (lockPreMatchPins)
                {
                    return preMatchPins;
                }
            }
            set
            {
                lock (lockPreMatchPins)
                {
                    preMatchPins = value;
                }
            }
        }

        const double STARTING_RES = 2;
        //private bool update_once = true;   //carina l'idea ma non penso la useremo,
                                            //a sto punto forse meglio usare due funzioni updateposition(ONCE/ALWAYS)
        const int INTERPOLATION_STEPS = 100;

        private int updateCtr = 0;
        private int tap_counter;

        public MainPageViewModel() 
        {
            //Constructor();
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////
        ///                             |                                   |                          ///
        ///                             |            BUG DI MAUI            |                          ///
        ///                             V                                   V                          ///
        //////////////////////////////////////////////////////////////////////////////////////////////////

        //DEVO FARLO PER UN BUG IN MAUI DOVE LE SHELL CONTENT CON DATA BINDING NON VENGONO MAI CREATI PIU DI UNA VOLTA
        //ANCHE SE USI TRANSIENT<>
        public void Constructor()
        {
            //runniamo il check dei permessi sul main thread
            MainThread.BeginInvokeOnMainThread(CheckANDSetPermission);

            Mapview = new();

            MapInitializer();
        }

        [RelayCommand]
        public async Task AbbandonaPartita()
        {
            MyUser.currPartita.LeaveLobby();

            Task.Delay(10).Wait();

            MyUser.CancelPositionRequest();
            ////////////////////
            //forse non va fatto??
            if (MyUser.isAdmin)
            {
                //event unsubscription
                Mapview.SingleTap -= creaPin;
            }
            foreach (var subscription in serverRegistrations)
            {
                subscription.Dispose();
            }
            //forse non va fatto??
            /////////////////////
            
            MyUser.currPartita.DisposeServerRegistrations();

            MyUser.currPartita = new();
            

            tap_counter = 0;
            WeakReferenceMessenger.Default.Send<UIChangeAlertStartPage>(new(LOBBY_HAS_BEEN_DELETED, NO_PAR));
            await AppShell.Current.GoToAsync("..", false);
            //var r = Shell.Current.Navigation.NavigationStack;
        }

        //forse va messa una condizione in modo tale che non runni sempre all avvio, tipo salvarci un bool su un file boh
        private async void CheckANDSetPermission()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

            if (status == PermissionStatus.Denied) //chiamiamo Geolocation perche fa la richiesta del gps 
                await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Lowest));
        }


        //legge risorse come nomi di file e le trasforma in byte array
        public static byte[] ReadResource(String filename)
        {
            byte[] result;
            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = assembly.GetManifestResourceNames()
                                .Single(str => str.EndsWith(filename));
            if (resourceName == null)
            {
                Console.WriteLine("///////////////////////" +
                    "IL NOME DEL FILE O IL FILE NON ESISTONO" +
                    "/////////////////////////");
            }
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    result = memoryStream.ToArray();
                }
            }
            return result;
        }

        private static TileLayer CreateTileLayer()
        {
            return new TileLayer(CreateTileSource()) { Name = "CartoDB.Voyager" };
        }

        private static HttpTileSource CreateTileSource()
        {
            //nel link z rappresenta il livello di zoom, x e y le coordinate, l'ultimo campo la risoluzione delle tiles
            return new HttpTileSource(new GlobalSphericalMercator(minZoomLevel: 3, maxZoomLevel: 18, name: null),
                "https://{s}.basemaps.cartocdn.com/rastertiles/voyager/{z}/{x}/{y}{@2x}.png",
                new[] { "a", "b", "c", "d" }, name: "CartoDB.Voyager");
        }

        public void MapInitializer()
        {
            MPoint initpos = new MPoint(MyUser.user.Position.Longitude,
                MyUser.user.Position.Latitude);
            Mapview.MyLocationLayer.UpdateMyLocation(initpos.ToMaui());
            Mapview.IsMyLocationButtonVisible = false;
            Mapview.MyLocationLayer.Opacity = 0;

            Mapview.Map?.Layers.Add(CreateTileLayer());

            Mapview.IsZoomButtonVisible = false;
            Mapview.MyLocationFollow = false;

            Mapview.Map.Home = n => n.NavigateTo(center:
                                      SphericalMercator.FromLonLat(initpos),
                                      STARTING_RES);
            Mapview.Pins.Clear();
            List<Pin> pinsToAdd = CreaPinsBindatiAllaMappa(PreMatchPins);
            foreach (Pin toAdd in pinsToAdd)
            {
                Mapview.Pins.Add(toAdd);
            }

            Task.Run(RestInitializer);
        }


        //QUA METTIAMO CHE TUTTI SI SPARGONO PARTENDO DALLA TUA POSIZIONE //FIGO
        private List<Pin> CreaPinsBindatiAllaMappa(IList<Pin> pins)
        {
            List<Pin> result = new List<Pin>();
            foreach(Pin toAdd in pins)
            {
                Pin newpin = new Pin(Mapview)
                {
                    Label = toAdd.Label,
                    Position = new(MyUser.user.Position.Latitude,
                                   MyUser.user.Position.Longitude),
                    Type = PinType.Icon,
                    Icon = toAdd.Icon,
                    Scale = 0.4F
                };
                result.Add(newpin);
            }
            return result;
        }

        private void RestInitializer()
        {
            Task.Run(Update_MyPosition_ALWAYS);

            Task.Run(aggiungiAltriGiocatoriAllaMappa);

            Task.Run(CreaAreaETana);
        }

        public void CreaAreaETana()
        {
            //se sei l'admin crei l'area
            if (MyUser.isAdmin)
            {
                Mapview.SingleTap += creaPin;
            }
            //senno' ricevi gli oggetti di gioco
            else
            {
                Task.Run(riceviOggettiDiGioco);
            }
        }

        public async Task OnUserOutside(List<User> UO)
        {
            tap_counter = 0;
            MyUser.currPartita.area = new();
            Mapview.Map.Layers.Remove(
                Mapview.Map.Layers.Single(x => x.Name.Equals(AreaGiocabile.NOME_LAYER_AREA))
                );
            StringBuilder s = new();
            UO.ForEach(u => s.Append(u.UserID + ", "));
            Console.WriteLine($"{s} sono fuori dall' area");
        }

        private void riceviOggettiDiGioco()
        {
            serverRegistrations.Add( 
                Connessione.con.On<string, string>("RiceviOggettiDiGioco", (coordarea, tana) =>
                    {
                        SerializableCoordinate[] ca = JsonSerializer.Deserialize<SerializableCoordinate[]>(coordarea);
                        SerializableCoordinate ct = JsonSerializer.Deserialize<SerializableCoordinate>(tana);
                        MyUser.currPartita.area.drawArea(ca, Mapview);
                        MyUser.currPartita.tana = new(ct, Mapview);
                    })
            );
        }

        public async void creaPin(object sender, Mapsui.UI.TappedEventArgs e)
        {
            Partita p = MyUser.currPartita;
            tap_counter++;
            // Get the coordinates of the tap
            MPoint worldPosition = Mapview.Viewport.ScreenToWorld(e.ScreenPosition);
            switch (tap_counter)
            {
                case < 6:
                    p.area.puntoBordo(worldPosition, Mapview);
                    break;
                case 6:
                    p.area.creaArea(Mapview);
                    List<User> UO = GameLogic.whoOutsideTheArea().Result;
                    if(UO.Count > 0)
                    {
                        await OnUserOutside(UO);
                    }
                    break;
                case 7:
                    p.tana = new(worldPosition);
                    if (GameLogic.IsInsideTheArea(p.area.bordi, worldPosition))
                    {
                        p.tana.drawArea(Mapview);
                        inviaOggettiDiGioco();

                        //StartGame(); //trovato il bug della doppia mappa?
                    }
                    else tap_counter--;
                    break;
            };

        }

        private void inviaOggettiDiGioco()
        {
            Partita p = MyUser.currPartita;

            SerializableCoordinate[] gioco = SerializableCoordinate.fromCoordinateArray(p.area.bordi.ToArray());
            SerializableCoordinate tana = new(p.tana.position.Longitude, p.tana.position.Latitude);

            string jcord = JsonSerializer.Serialize<SerializableCoordinate[]>
                (gioco, new JsonSerializerOptions
                {
                    NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
                    PropertyNameCaseInsensitive = true
                });

            string jtana = JsonSerializer.Serialize<SerializableCoordinate>
                (tana, new JsonSerializerOptions
                {
                    NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
                    PropertyNameCaseInsensitive = true
                });

            Connessione.con.InvokeAsync("InviaOggettiDiGioco",
                arg1: MyUser.currPartita.Cod_partita,
                arg2: jcord,
                arg3: jtana);
        }

        private void aggiungiAltriGiocatoriAllaMappa()
        {
            serverRegistrations.Add( 
                Connessione.con.On<string>("PositionReceived", (receiveduser) =>
                    {
                        SerializableUser user = JsonSerializer.Deserialize<SerializableUser>(receiveduser);
                        Console.WriteLine($"/////////Posizione ricevuta da:{user.UserID} , " +
                            $"lat:{user.Position.Latitude}, lon: {user.Position.Longitude}");
                        if (user.UserID != MyUser.user.UserID)
                        {
                            //bool trovato = false; //ho gia tutti i players
                            Position position = new(user.Position.Latitude, user.Position.Longitude);
                            //se trovo l'utente aggiorno la sua posizione
                            foreach (Pin p in Mapview.Pins)
                            {
                                if (user.UserID.Equals(p.Label))
                                {
                                    Interpolate(p, position); //animazione piu fluida

                                    //aggiorno lo user nella lista della partita
                                    //ma che sto facendo AIUTO!
                                    User alreadyIn = MyUser.currPartita.Players.Where((x) =>
                                    x.UserID.Equals(p.Label)).First();

                                    alreadyIn.Position = new(position.Latitude, position.Longitude);
                                    alreadyIn.IsCercatore = user.IsCercatore;
                                }
                            }
                            //non serve piu aggiungere perche in toeria non puo entrare gente nuova se la partita è in corso

                            //altrimenti ne creo uno nuovo(di pin)
                            //if (!trovato)
                            //{
                            //    Pin userPin = new Pin(mapView)
                            //    {
                            //        Label = user.UserID,
                            //        Position = position,
                            //        Type = PinType.Icon,
                            //        Icon = user.UserIcon,
                            //        Scale = 0.4F
                            //    };
                            //    mapView.Pins.Add(userPin);

                            //    //creo loggetto user e lo aggiungo alla lista dei players nella partita

                            //    User justReceived = new(user.Nickname, user.UserID, new(position.Latitude, position.Longitude))
                            //    {
                            //        UserPin = userPin
                            //    };

                            //    MyUser.currPartita.Players.Add(justReceived);

                            //}
                        }
                    })
                );
        }

        //updates the position once
        //public async Task Update_MyPosition_ONCE()
        //{   
        //    await MyUser.Get_Position();
        //    Position p = new(MyUser.user.Position.Latitude, MyUser.user.Position.Longitude);

        //    Console.WriteLine($"Position updated from {MyUser.user.UserID} {updateCtr} times (single update)");
        //    mapView.MyLocationLayer.UpdateMyLocation(p, false);
        //    mapView.Navigator.FlyTo(new MPoint(MyUser.user.Position.Longitude,
        //       MyUser.user.Position.Latitude), 3, 3000);
        //    update_once = false;
        //    mapView.IsMyLocationButtonVisible = true;
        //    updateCtr++;
        //}

        //updates the position finche non casca il mondo
        public async Task Update_MyPosition_ALWAYS()
        {
            //finchè vogliamo la posizione la queriamo, possiamo in qualsiasi momento
            //smettere di chiederla
            bool firstupdate = true;
            while (MyUser.SEND_POSITION)
            {
                await MyUser.Get_Position();
                Position p = MyUser.user.UserPin.Position;
                //ma serve ancora sta cosa???? ----- Carlo: SI zi
                Mapview.MyLocationLayer.UpdateMyLocation(p, false);
                if (firstupdate)
                {
                    Mapview.Navigator.FlyTo(SphericalMercator.FromLonLat(
                        new MPoint(MyUser.user.Position.Longitude,
                                    MyUser.user.Position.Latitude)), STARTING_RES, 3000);
                    User u = MyUser.user;
                    MyUser.user.UserPin = new Pin(Mapview)
                    {
                        Label = u.UserID,
                        Position = new Position(u.Position.Latitude, u.Position.Longitude),
                        Type = PinType.Icon,
                        Icon = u.UserIcon,
                        Scale = 0.4F
                    };
                    Mapview.Pins.Add(MyUser.user.UserPin);
                    firstupdate = false;
                    Mapview.IsMyLocationButtonVisible = true;
                }
                updateCtr++;
                Console.WriteLine($"Position updated from {MyUser.user.UserID} {updateCtr} times (continuos update)");
            }
        }


        //Per interpolare le posizioni ricevute dagli utenti
        public async void Interpolate(Pin p, Position newPos)
        {
            Position oldPos = p.Position;

            for (double i = 1; i <= INTERPOLATION_STEPS; i++)
            {
                p.Position = Interpolate_points_scalar(oldPos, newPos, i);
                await Task.Delay(MyUser.SEND_POS_DELAY / INTERPOLATION_STEPS);
            }
        }
        //funzione di supporto
        //ritorna il punto che si trova a (scale/INTERPOLATION_STEPS) fra i due punti in input
        //tipo il punto a 3/10 di distanza tra i due punti estremi del segmento
        public Position Interpolate_points_scalar(Position p1, Position p2, double scale)
        {
            double x1 = p1.Longitude;
            double y1 = p1.Latitude;
            double x2 = p2.Longitude;
            double y2 = p2.Latitude;

            double x3 = (x1 + ((scale / INTERPOLATION_STEPS) * (x2 - x1)));
            double y3 = (y1 + ((scale / INTERPOLATION_STEPS) * (y2 - y1)));

            Position newPos = new(y3, x3);
            return newPos;
        }


        /////////////////////////////////////////////////////////////////////////////////
        ///ALTRO MODO DI IMPLEMENTARE MA DEVI CHIAMARE DUE VOLTE UPDATE POSITION PERCHE//
        // UPDATE MAP TO POS VA FATTA MEGLIO/////////////////////////////////////////////
        /*
        * 
        * //IF YOU WANT TO DO IT ONCE SET THE update_once = true BEFORE CALLING this task
        * 
        public async Task Update_MyPosition()
        if(update_once)
        {
            await mypos.Get_Position();
            Position p = MyUser.user.position;
            mapView.MyLocationLayer.UpdateMyLocation(p, true);
            update_once = false;
            updateCtr++;
            Console.WriteLine($"Position updated {updateCtr} times (single update)");
        }
        else
        //updates the position finche non casca il mondo
        {
            //finchè vogliamo la posizione la queriamo, possiamo in qualsiasi momento
            //smettere di chiederla
            while (want_position)
            {
                await mypos.Get_Position();
                Position p = MyUser.user.position;
                mapView.MyLocationLayer.UpdateMyLocation(p, true);
                updateCtr++;
                Console.WriteLine($"Position updated {updateCtr} times (continuos update)");
            }
        }
           
        */


        //easier way to add pins
        public static void AddPin(Position pos, String label, Microsoft.Maui.Graphics.Color c, MapView mapview)

        {
            mapview.Pins.Add(new Pin(mapview)
            {
                Label = label,
                Position = pos,
                Type = PinType.Pin,
                Color = c,
                Scale = 0.35F,
            });
        }

        public static void AddPinFromFile(Position pos, String label, string iconFilename, float scale, MapView mapview)

        {
            mapview.Pins.Add(new Pin(mapview)
            {
                Label = label,
                Position = pos,
                Type = PinType.Icon,
                Scale = scale,
                Icon = ReadResource(iconFilename)
            });
        }




        // NON FUNGE -> DA VEDERE

        //public static MemoryLayer creaLayerPins()
        //{
        //    return new MemoryLayer {
        //        Name = "Points",
        //        IsMapInfoLayer = true,
        //        Features = creaPins(),
        //        Style = CreaStile()
        //    }; 

        //}

        //public static IEnumerable<IFeature> creaPins()
        //{
        //    var listaPins = new List<MPoint>();
        //    listaPins.Add(SphericalMercator.FromLonLat(new MPoint(41.7509, 12.33964)));

        //    return listaPins.Select(p =>
        //    {
        //        var feature = new PointFeature(p);
        //        feature["Name"] = "Tana";
        //        return feature;
        //    });

        //}
        //private static SymbolStyle CreaStile()
        //{
        //    var assembly = typeof(OurMapController).Assembly;
        //    var image = assembly.GetManifestResourceStream("ProjApp.Resources.Images.pin_icon.png");
        //    var ID = BitmapRegistry.Instance.Register(image);


        //    return new SymbolStyle{ BitmapId = ID, SymbolScale = 0.50, SymbolOffset = new Offset(0, 32 * 0.5) };
        //}

    }
}