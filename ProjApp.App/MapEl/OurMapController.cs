using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Android.Icu.Number;
using BruTile.Predefined;
using BruTile.Web;
using Mapsui;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.Tiling.Layers;
using Mapsui.UI.Maui;
using Mapsui.UI.Maui.Extensions;
using Microsoft.AspNetCore.SignalR.Client;
using NetTopologySuite.Geometries;
using Polygon = NetTopologySuite.Geometries.Polygon;
using Position = Mapsui.UI.Maui.Position;
using ProjApp.MapEl.GPS;
using Microsoft.Maui.Graphics;
using Mapsui.Nts.Extensions;
using ProjApp.Gioco;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Text.Json;
using Location = Microsoft.Maui.Devices.Sensors.Location;

namespace ProjApp.MapEl
{
    public class OurMapController : MapControl
    {
        private MapView mapView = new();
        private MyUser myuser;

        const double STARTING_RES = 2;
        private bool update_once = true;   //carina l'idea ma non penso la useremo,
                                           //a sto punto forse meglio usare due funzioni updateposition(ONCE/ALWAYS)
        const int INTERPOLATION_STEPS = 100;
        
        private bool want_position = true;
        private int updateCtr = 0;
        
        //SignalR Parametri
        private HubConnection connection_nelMC;
        const int SEND_POS_DELAY = 3000;
        
        private bool want_sendposition = true;

        //legge risorse come nomi di file e le trasforma in byte array
        public static byte[] ReadResource(Assembly assembly, String filename)
        {
            byte[] result;
            string resourceName = assembly.GetManifestResourceNames()
                                .Single(str => str.EndsWith(filename));
            if (resourceName == null)
            {
                throw new ArgumentNullException("///////////////////////" +
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

        public static TileLayer CreateTileLayer()
        {
            return new TileLayer(CreateTileSource()) { Name = "CartoDB.Voyager" };
        }

        private static HttpTileSource CreateTileSource()
        {
            //nel link z rappresenta il livello di zoom, x e y le coordinate, l'ultimo campo la risoluzione delle tiles
            return new HttpTileSource(new GlobalSphericalMercator(minZoomLevel: 0, maxZoomLevel: 20, name: null),
                "https://{s}.basemaps.cartocdn.com/rastertiles/voyager/{z}/{x}/{y}{@2x}.png",
                new[] { "a", "b", "c", "d" }, name: "CartoDB.Voyager");
        }

        public MapView MapInitializer(HubConnection _connection)
        {
            myuser = new MyUser(mapView);
            connection_nelMC = _connection;
            mapView.IsMyLocationButtonVisible = false;

            //PROVA//
            Task.Run(() => this.creaPartita(_connection));
            ////////
            Task.Run(() => this.Update_MapToPos()).Wait();
            Task.Run(() => this.Update_MyPosition_ALWAYS());

            //Task.Run(() => this.inviaPosSignalR());
            Task.Run(() => this.aggiungiAltriGiocatoriAllaMappa());

            mapView.Map?.Layers.Add(OurMapController.CreateTileLayer());

            mapView.Map?.Layers.Add(CreateCustomLayer("RandomPolys"));

            mapView.IsZoomButtonVisible = false;
            mapView.MyLocationFollow = false;

            mapView.Map.Home = n => n.NavigateTo(center:
                                      SphericalMercator.FromLonLat(new MPoint(
                                      12.340445071924254, 41.74608176704198)),
                                      resolution: STARTING_RES);

            //mapView.Map.Layers.Add(creaLayerPins());

            //PROVA LEVALAAAAAAAAAAAAAAAAAAA
            User userFake = new User("O", "ulala", new Location(41.767523, 12.359897), mapView);
            mapView.Pins.Add(userFake.UserPin);
            //

            AddPin(mapView, new Position(41.746168, 12.340037), "Casetta", Colors.Aqua);
            AddPin(mapView, new Position(41.767523, 12.359897), "Carlium", Colors.Red);
            AddPin(mapView, new Position(41.757395, 12.353765), "Nardium", Colors.Orange);


            return mapView;

        }

        private async void creaPartita(HubConnection con)
        {
            //aspetto che si connetta prima (temporaneao)
            await Task.Delay(7000);
            Partita p = new(con);
            p.CreateLobby();
            p.StartGame();
            con.On("GameStarted", () => {
                Console.WriteLine("GameStarted message from server");
                Task.Run(() => this.inviaPosSignalR());
            });

            
        }




        //metodo che crea layer generici
        public ILayer CreateCustomLayer(string name)
        {
            return new Layer(name)
            {
                DataSource = new MemoryProvider(CreaListaPoligoni().ToFeatures()),
                Style = new VectorStyle
                {
                    Fill = new Mapsui.Styles.Brush(new Mapsui.Styles.Color(205, 255, 255, 100)),
                    Outline = new Pen
                    {
                        Color = Mapsui.Styles.Color.Cyan,
                        Width = 2,
                        PenStyle = PenStyle.DashDotDot,
                        PenStrokeCap = PenStrokeCap.Round
                    }
                }
            };

        }


        //crea una lista di poligoni
        public List<Polygon> CreaListaPoligoni()
        {
            var result = new List<Polygon>();

            var poly1 = new NetTopologySuite.Geometries.Polygon(new LinearRing
                (new[]
                    {
                        new Position( 41.731604 , 12.338212 ).ToMapsui().ToCoordinate(),
                        new Position( 41.749755 , 12.314546 ).ToMapsui().ToCoordinate(),
                        new Position( 41.768707 , 12.325532 ).ToMapsui().ToCoordinate(),
                        new Position( 41.756030 , 12.362954 ).ToMapsui().ToCoordinate(),
                        new Position( 41.731604 , 12.338212 ).ToMapsui().ToCoordinate()
                    })
                );

            var poly2 = new NetTopologySuite.Geometries.Polygon(new LinearRing
                (new[]
                    {
                        new Position( 41.761940 , 12.337542 ).ToMapsui().ToCoordinate(),
                        new Position( 41.764191 , 12.340910 ).ToMapsui().ToCoordinate(),
                        new Position( 41.760128 , 12.340559 ).ToMapsui().ToCoordinate(),
                        new Position( 41.761940 , 12.337542 ).ToMapsui().ToCoordinate()
                    })
                );
            result.Add(poly1);
            result.Add(poly2);
            return result;
        }

        //FINE METODI TEMPORANEI SIGNALR

        public async void inviaPosSignalR()
        {
            while (want_sendposition)
            {
                if (connection_nelMC.State.Equals(HubConnectionState.Connected))
                {
                    string jsonUser = JsonSerializer.Serialize<User>(MyUser.user,
                          new JsonSerializerOptions
                          {
                              NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
                              PropertyNameCaseInsensitive = true
                          });

                    await connection_nelMC.InvokeAsync("SendPosition",
                          arg1: jsonUser,
                          //Codice lobby
                          arg2: MyUser.currPartita);
                }
                await Task.Delay(SEND_POS_DELAY);
            }
        }

        private void aggiungiAltriGiocatoriAllaMappa()
        {
            connection_nelMC.On<string>("PositionReceived", (receiveduser) =>
            {
                SerializableUser user = JsonSerializer.Deserialize<SerializableUser>(receiveduser);
                Console.WriteLine($"/////////Posizione ricevuta da:{user.UserID} , " +
                    $"lat:{user.Position.Latitude}, lon: {user.Position.Longitude}");
                if (user.UserID != MyUser.user.UserID) {
                    bool trovato = false;
                    Position position = new(user.Position.Latitude, user.Position.Longitude);
                    //se trovo l'utente aggiorno la sua posizione
                    foreach (Pin p in mapView.Pins)
                    {
                        if (user.UserID.Equals(p.Label))
                        {
                            trovato = true;
                            Interpolate(p, position); //animazione piu fluida
                        }
                    }
                    //altrimenti ne creo uno nuovo (di pin)
                    if (!trovato)
                    {
                        mapView.Pins.Add(new Pin(mapView)
                            {
                                Label = user.UserID,
                                Position = position,
                                Type = PinType.Icon,
                                Icon = user.UserIcon,
                                Scale = 0.6F
                            });
                    }
                }
            });
        }

        //updates the position ONCE and animates the fly to the new position
        public async void Update_MapToPos()
        {
            
            await Update_MyPosition_ONCE();    
            mapView.Navigator.FlyTo(
                SphericalMercator.FromLonLat(new MPoint(MyUser.user.Position.Longitude, MyUser.user.Position.Latitude)), 3, 5000);
            mapView.IsMyLocationButtonVisible = true;

        }

        //updates the position once
        public async Task Update_MyPosition_ONCE()
        {   
            await myuser.Get_Position();
            Position p = MyUser.user.UserPin.Position;
            mapView.MyLocationLayer.UpdateMyLocation(p, true);
            update_once = false;
            updateCtr++;
            Console.WriteLine($"Position updated from {MyUser.user.UserID} {updateCtr} times (single update)");
        }

        //updates the position finche non casca il mondo
        public async Task Update_MyPosition_ALWAYS()
        {
            //finchè vogliamo la posizione la queriamo, possiamo in qualsiasi momento
            //smettere di chiederla
            while (want_position)
            {
                await myuser.Get_Position();
                Position p = MyUser.user.UserPin.Position;
                mapView.MyLocationLayer.UpdateMyLocation(p, true);
                updateCtr++;

                Console.WriteLine($"Position updated from {MyUser.user.UserID} {updateCtr} times (continuos update)");
            }
        }


        //Per interpolare le posizioni ricevute dagli utenti
        public async void Interpolate(Pin p, Position newPos)
        {
            Position oldPos = p.Position;

            for (double i = 1; i<=INTERPOLATION_STEPS; i++)
            {
                p.Position = Interpolate_points_scalar(oldPos, newPos, i);
                await Task.Delay(SEND_POS_DELAY/INTERPOLATION_STEPS);
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

            Position newPos = new Position(y3, x3);
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
        public static void AddPin(MapView mapView, Position pos, String label, Microsoft.Maui.Graphics.Color c)
        {
            mapView.Pins.Add(new Pin(mapView)
            {
                Label = label,
                Position = pos,
                Type = PinType.Pin,
                Color = c,
                Scale = 0.35F,
            }) ;
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
