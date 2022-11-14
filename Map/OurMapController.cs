using System;
using System.Collections.Generic;
using System.Linq;
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
using ProjApp.Map.GPS;

using Color = Mapsui.Styles.Color;

namespace ProjApp.Map
{
    public static class OurMapController
    {
        const double STARTING_RES = 0.1;

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

        public static MapView MapInitializer()
        {
            MapView mapView = new();
            mapView.Map?.Layers.Add(OurMapController.CreateTileLayer());

            Position startingPos = new MyPosition().position;
            mapView.MyLocationLayer.UpdateMyLocation(startingPos, true);
            mapView.Map.Home = n => n.NavigateTo(center:
                                        SphericalMercator.FromLonLat(new MPoint(
                                        startingPos.Longitude, startingPos.Latitude)),
                                        resolution: STARTING_RES);
            mapView.IsZoomButtonVisible = false;
            mapView.IsMyLocationButtonVisible = true;
            mapView.Map.Layers.Add(creaLayerPins());
            return mapView;

        }




        public static MemoryLayer creaLayerPins()
        {
            return new MemoryLayer {
                Name = "Points",
                IsMapInfoLayer = true,
                Features = creaPins(),
                Style = CreaStile()
            }; 
            
        }
        
        public static IEnumerable<IFeature> creaPins()
        {
            var listaPins = new List<MPoint>();
            listaPins.Add(SphericalMercator.FromLonLat(new MPoint(41.7509, 12.33964)));

            return listaPins.Select(p =>
            {
                var feature = new PointFeature(p);
                feature["Name"] = "Tana";
                feature.Styles.Add(CreaStile());
                return feature;
            });

        }
        private static IStyle CreaStile()
        {
            return new SymbolStyle { SymbolScale = 0.2, Fill = new Mapsui.Styles.Brush(new Color(40, 40, 40)) };
        }
    }
}
