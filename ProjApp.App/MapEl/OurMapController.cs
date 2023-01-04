﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

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

        const double STARTING_RES = 2;
        private bool update_once = true;   //carina l'idea ma non penso la useremo,
                                           //a sto punto forse meglio usare due funzioni updateposition(ONCE/ALWAYS)
        const int INTERPOLATION_STEPS = 100;
        
        private bool want_position = true;
        private int updateCtr = 0;
        private bool flyToIsRunning = false;
        
        //SignalR Parametri
        const int SEND_POS_DELAY = 3000;
        
        private static bool want_sendposition = true;

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

        public MapView MapInitializer()
        {
            Task.Run( async () => {

                MyUser.BuildMyUser(mapView);
                mapView.IsMyLocationButtonVisible = false;
                mapView.MyLocationLayer.Opacity = 0;

                mapView.Pins.Add(MyUser.user.UserPin);

                mapView.Map?.Layers.Add(OurMapController.CreateTileLayer());

                mapView.Map?.Layers.Add(CreateCustomLayer("RandomPolys"));
                mapView.IsZoomButtonVisible = false;
                mapView.MyLocationFollow = false;
                mapView.Map.Home = n => n.NavigateTo(center:
                                          SphericalMercator.FromLonLat(new MPoint(
                                          MyUser.user.Position.Longitude, MyUser.user.Position.Latitude)),
                                          resolution: STARTING_RES);

                await Update_MyPosition_ONCE();
                Update_MyPosition_ALWAYS();
            }).Wait();

            Partita part = new(MainPage._connection);

            Task.Run(() => this.serverMessages(part));
            Task.Run(() => this.isGameStarted());

            Task.Run(() => this.aggiungiAltriGiocatoriAllaMappa());


            //PROVA//
            Task.Run(() => this.creaPartitaEGioca(part));
            ////////

            return mapView;
        }
        
        private void waitConnected()
        {
            while (!MainPage._connection.State.Equals(HubConnectionState.Connected)){
                Task.Delay(1000).Wait();
            }
        }

        private void serverMessages(Partita p)
        {
            int count = 0;

            MainPage._connection.On<string>("ServerMessage", (mess) =>
            {
                Console.WriteLine($"///SERVER///::  {mess}");
                count++;
                if (count == 2)
                {
                    p.StartGame();
                    p.GiocoInCorso = true;
                }

            });
        }


        private void isGameStarted()
        {
            MainPage._connection.On<bool>("GameStarted", (isCacciatore) =>
            {
                if (isCacciatore)
                {
                    Console.WriteLine("GameStarted message from server, SEI IL CACCIATORE");
                }
                else
                {
                    Console.WriteLine("GameStarted message from server, non sei il cacciatore");
                }
            });
        }

        private async void creaPartitaEGioca(Partita p)
        {
            await Task.Delay(7000);
            p.CreateLobby();
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

        public static async void inviaPosSignalR()
        {
            while (want_sendposition)
            {
                if (MainPage._connection.State.Equals(HubConnectionState.Connected))
                {
                    string jsonUser = JsonSerializer.Serialize<User>(MyUser.user,
                          new JsonSerializerOptions
                          {
                              NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
                              PropertyNameCaseInsensitive = true
                          });

                    await MainPage._connection.InvokeAsync("SendPosition",
                          arg1: jsonUser,
                          //Codice lobby
                          arg2: MyUser.currPartita);
                }
                await Task.Delay(SEND_POS_DELAY);
            }
        }

        private void aggiungiAltriGiocatoriAllaMappa()
        {
            MainPage._connection.On<string>("PositionReceived", (receiveduser) =>
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

        //updates the position once
        public async Task Update_MyPosition_ONCE()
        {   
            Position p = new(MyUser.user.Position.Latitude, MyUser.user.Position.Longitude);
            if (p.Longitude == 0 && p.Latitude == 0)
            {
                await MyUser.Get_Position();
                Console.WriteLine($"Position updated from {MyUser.user.UserID} {updateCtr} times (single update)");
            }
            else
            {
                Console.WriteLine($"Position retrieved from file for user {MyUser.user.UserID} (single update)");
            }

            mapView.MyLocationLayer.UpdateMyLocation(p, false);
            mapView.Navigator.NavigateTo(new MPoint(MyUser.user.Position.Longitude,
                MyUser.user.Position.Latitude), STARTING_RES, 2000);
            update_once = false;
            mapView.IsMyLocationButtonVisible = true;
            updateCtr++;
        }
       
        //updates the position finche non casca il mondo
        public async Task Update_MyPosition_ALWAYS()
        {
            //finchè vogliamo la posizione la queriamo, possiamo in qualsiasi momento
            //smettere di chiederla
            while (want_position)
            {
                await MyUser.Get_Position();
                Position p = MyUser.user.UserPin.Position;
                mapView.MyLocationLayer.UpdateMyLocation(p, false);
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
