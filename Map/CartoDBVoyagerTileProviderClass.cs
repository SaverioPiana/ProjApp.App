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
            return new TileLayer(CreateTileSource()) { Name = "OpenStreetMap" };
        }

        private static HttpTileSource CreateTileSource()
        {
            return new HttpTileSource(new GlobalSphericalMercator(),
                "https://{s}.basemaps.cartocdn.com/rastertiles/voyager/{z}/{x}/{y}{r}.png",
                new[] { "a", "b", "c", "d" }, name: "OpenStreetMap");
        }
    }
}
