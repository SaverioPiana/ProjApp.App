using System.Collections.Generic;
using System.Linq;
using Mapsui.Layers;
using Mapsui.Styles;
using Mapsui.Utilities;
using Mapsui.Extensions;
using Mapsui.Tiling;
using System.Threading.Tasks;
using Mapsui;
using Color = Mapsui.Styles.Color;
using Brush = Mapsui.Styles.Brush;
using Mapsui.UI.Maui;
using Style = Mapsui.Styles.Style;
using Mapsui.Styles.Thematics;
using ProjApp.Gioco;
using Mapsui.Providers;
using System.Reflection;
using NetTopologySuite.Geometries;
using Polygon = NetTopologySuite.Geometries.Polygon;
using Mapsui.Nts.Extensions;
using Svg.FilterEffects;

namespace ProjApp.MapEl
{   
    public static class CustomLayerExtensions
    {
        //crea un layer generico (style puo essere nullo)
        public static ILayer CreateCustomLayer(string name, IEnumerable<IFeature> features, Style style)
        {
            ILayer custom = new Layer(name)
            {   
                DataSource = new MemoryProvider(features),
                Style = style
            };
            return custom;
        }

        //metodo che crea layer con poligono custom
        public static ILayer CreatePoligonoLayer(string name, Coordinate[] punti, Style style)
        {
            return CreateCustomLayer(name, CreaPoligono(punti).ToFeatures(), style);
        }


        //crea una lista di poligoni, l'ultima e la prima coordinata devono essere uguali
        public static List<Polygon> CreaPoligono(Coordinate[] punti)
        {
            var result = new List<Polygon>();

            var poly1 = new NetTopologySuite.Geometries.Polygon(new LinearRing(punti));

            result.Add(poly1);

            return result;
        }

        public static IStyle PositionDot()
        {
            return new SymbolStyle { SymbolScale = 0.2, Fill = new Brush(new Color(40, 40, 40)) };
        }

        public static SymbolStyle CreateBitmapStyle(string filename, double scale, RelativeOffset relOff)
        {
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = assembly.GetManifestResourceNames()
                                .Single(str => str.EndsWith(filename));
            var tanaicon = assembly.GetManifestResourceStream(resourceName);
            var bitmapId = BitmapRegistry.Instance.Register(tanaicon);
            return new SymbolStyle { BitmapId = bitmapId, SymbolScale = scale, SymbolOffset = relOff};
        }

        public static SymbolStyle CreateSvgStyle(string embeddedResourcePath, double scale)
        {
            var bitmapId = typeof(CustomLayerExtensions).LoadSvgId(embeddedResourcePath);
            return new SymbolStyle { BitmapId = bitmapId, SymbolScale = scale, SymbolOffset = new RelativeOffset(0.0, 0.0) };
        }
        
    }
}