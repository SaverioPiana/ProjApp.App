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
using Mapsui.Styles;
using Mapsui.Tiling.Layers;
using Mapsui.UI.Maui;
using Mapsui.UI.Maui.Extensions;
using Microsoft.AspNetCore.SignalR.Client;
using ProjApp.Map.GPS;
using Color = Microsoft.Maui.Graphics.Color;

namespace ProjApp.Map
{
    public class OurMapController
    {
        MyPosition mypos = new();
        MapView mapView = new();

        const double STARTING_RES = 2;
        private bool update_once = true;   //carina l'idea ma non penso la useremo,
                                           //a sto punto forse meglio usare due funzioni updateposition(ONCE/ALWAYS)
        const int INTERPOLATION_STEPS = 100;
        
        private bool want_position = true;
        private int updateCtr = 0;
        
        //SignalR Parametri
        private HubConnection connection_nelMC;
        private bool want_sendposition = true;
        const int SEND_POS_DELAY = 3000;



        public static TileLayer CreateTileLayer()
        {
            return new TileLayer(CreateTileSource()) { Name = "CartoDB.Voyager" };
        }

        private static HttpTileSource CreateTileSource()
        {
            //nel link z rappresenta il livello di zoom, x e y le coordinate, l'ultimo campo la risoluzione delle tiles
            return new HttpTileSource(new GlobalSphericalMercator(minZoomLevel: 10, maxZoomLevel: 20, name: null),
                "https://{s}.basemaps.cartocdn.com/rastertiles/voyager/{z}/{x}/{y}{@2x}.png",
                new[] { "a", "b", "c", "d" }, name: "CartoDB.Voyager");
        }

        public MapView MapInitializer(HubConnection _connection)
        {

            connection_nelMC = _connection;

            Task.Run(() => this.Update_MapToPos()).Wait();
            Task.Run(() => this.Update_MyPosition_ALWAYS());

            Task.Run(() => this.inviaPosSignalR());
            Task.Run(() => this.aggiungiAltriGiocatoriAllaMappa());

            mapView.Map?.Layers.Add(OurMapController.CreateTileLayer());

            mapView.IsZoomButtonVisible = false;
            mapView.IsMyLocationButtonVisible = true;
            mapView.MyLocationFollow = false;
            
            
            mapView.Map.Home = n => n.NavigateTo(center:
                                      SphericalMercator.FromLonLat(new MPoint(
                                      12.340445071924254, 41.74608176704198)),
                                      resolution: STARTING_RES);


            //mapView.Map.Layers.Add(creaLayerPins());


            AddPin(mapView, new Position(41.746168, 12.340037), "Casetta", Colors.Aqua);
            AddPin(mapView, new Position(41.767523, 12.359897), "Carlium", Colors.Red);
            AddPin(mapView, new Position(41.757395, 12.353765), "Nardium", Colors.Orange);


            return mapView;

        }

        //SIGNALR METODI TEMPORANEI
        private async void inviaPosSignalR()
        {
            while (want_sendposition)
            {
                if (connection_nelMC.State.Equals(HubConnectionState.Connected)) { 
                    await connection_nelMC.InvokeAsync("SendPosition",
                          arg1: DeviceInfo.Name,
                          arg2: MyPosition.position.Latitude,
                          arg3: MyPosition.position.Longitude);
                 }
                await Task.Delay(SEND_POS_DELAY);
            }

        }


        //FINE METODI TEMPORANEI SIGNALR
        private void aggiungiAltriGiocatoriAllaMappa()
        {

            string my_user = DeviceInfo.Name;

            connection_nelMC.On<string, double, double>("PositionReceived", (user, lat, lon) =>
            {
                Console.WriteLine($"/////////Posizione ricevuta da:{user} , lat:{lat}, lon: {lon}");
                if (user != my_user) {
                    bool trovato = false;
                    Position position = new(lat, lon);
                    //se trovo l'utente aggiorno la sua posizione
                    foreach (Pin p in mapView.Pins)
                    {
                        if (user == p.Label)
                        {
                            trovato = true;
                            Interpolate(p, position); //animazione piu fluida
                        }
                    }
                    //altrimenti ne creo uno nuovo (di pin)
                    if (!trovato)
                    {
                        Random r = new();
                        AddPin(mapView, position, user,
                            Color.FromRgb(r.Next(256), r.Next(256), r.Next(256)));
                    }
                }
            });
        }

        //updates the position ONCE and animates the fly to the new position
        public async void Update_MapToPos()
        {
            {
                await Update_MyPosition_ONCE();    
                mapView.Navigator.FlyTo(
                    SphericalMercator.FromLonLat(new MPoint(MyPosition.position.Longitude, MyPosition.position.Latitude)), 3, 5000);
            }

        }

        //updates the position once
        public async Task Update_MyPosition_ONCE()
        {   
            await mypos.Get_Position();
            Position p = MyPosition.position;
            mapView.MyLocationLayer.UpdateMyLocation(p, true);
            update_once = false;
            updateCtr++;
            Console.WriteLine($"Position updated from {DeviceInfo.Name} {updateCtr} times (single update)");
        }

        //updates the position finche non casca il mondo
        public async Task Update_MyPosition_ALWAYS()
        {
            //finchè vogliamo la posizione la queriamo, possiamo in qualsiasi momento
            //smettere di chiederla
            while (want_position)
            {
                await mypos.Get_Position();
                Position p = MyPosition.position;
                mapView.MyLocationLayer.UpdateMyLocation(p, true);
                updateCtr++;

                Console.WriteLine($"Position updated from {DeviceInfo.Name} {updateCtr} times (continuos update)");
            }
        }


        //Per interpolare le posizioni ricevute dagli utenti
        public async void Interpolate(Pin p, Position newPos)
        {
            Position oldPos = p.Position;

            for (double i = 1; i<=INTERPOLATION_STEPS; i++)
            {
                p.Position = await Interpolate_points_scalar(oldPos, newPos, i);
                await Task.Delay(SEND_POS_DELAY/INTERPOLATION_STEPS);
            }
        }
        //funzione di supporto
        //ritorna il punto che si trova a (scale/INTERPOLATION_STEPS) fra i due punti in input
        //tipo il punto a 3/10 di distanza tra i due punti estremi del segmento
        public async Task<Position> Interpolate_points_scalar(Position p1, Position p2, double scale) 
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
            Position p = MyPosition.position;
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
                Position p = MyPosition.position;
                mapView.MyLocationLayer.UpdateMyLocation(p, true);
                updateCtr++;
                Console.WriteLine($"Position updated {updateCtr} times (continuos update)");
            }
        }
           
        */


        //easier way to add pins
        public static void AddPin(MapView mapView, Position pos, String label, Color c)
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
