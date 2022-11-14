using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BruTile.Predefined;
using BruTile.Web;
using Mapsui.Layers;
using Mapsui.Tiling.Layers;

namespace ProjApp.Map
{
    public static class CartoDBVoyagerTileProviderClass
    {
        public static TileLayer CreateTileLayer()
        {
            return new TileLayer(CreateTileSource()) { Name = "CartoDB.Voyager" };
        }

        private static HttpTileSource CreateTileSource()
        {
            //nel link z rappresenta il livello di zoom, x e y le coordinate, l'ultimo campo la risoluzione delle tiles
            return new HttpTileSource(new GlobalSphericalMercator(minZoomLevel: 10, maxZoomLevel: 20, name: null ),
                "https://{s}.basemaps.cartocdn.com/rastertiles/voyager/{z}/{x}/{y}{@2x}.png",
                new[] { "a", "b", "c", "d" }, name: "CartoDB.Voyager") ;
        }
    }
}
