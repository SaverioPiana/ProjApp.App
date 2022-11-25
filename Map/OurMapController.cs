using System;
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
using Mapsui.Styles;
using Mapsui.Tiling.Layers;
using Mapsui.UI.Maui;
using Mapsui.UI.Maui.Extensions;
using ProjApp.Map.GPS;
using static Google.Android.Material.Tabs.TabLayout;
using Color = Microsoft.Maui.Graphics.Color;

namespace ProjApp.Map
{
    public class OurMapController
    {
        MyPosition mypos = new();
        MapView mapView = new();

        const double STARTING_RES = 2;

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

        public MapView MapInitializer()
        {
            Task.Run(() => this.Update_MyPosition());
            Task.Run(() => this.Update_MapToPos());


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
        public async Task Update_MapToPos()
        {
            {
                Thread.Sleep(5000);
                mapView.Navigator.FlyTo(
                    SphericalMercator.FromLonLat(new MPoint(MyPosition.position.Longitude, MyPosition.position.Latitude)), 3 , 5000);
                

            }

        }
        public async Task Update_MyPosition()
        {
            /////ATTENTO FA SCHIFO WHILE TRUE////////
            while (true) {
                Position p = mypos.returnPosition().Result;
                mapView.MyLocationLayer.UpdateMyLocation(p, true);
                Thread.Sleep(3000);

            }

        }
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
