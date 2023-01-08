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

namespace ProjApp.MapEl
{   //PROBABILMENTE LA USEREMO BOH
    public class CustomLayer
    {
        public IList<IFeature> features { get; set; }
        public Layer userlayer { get; set; }

        public CustomLayer()
        {
            features = new List<IFeature>();
        }
        
        public void CreateLayer()
        {
            userlayer = new Layer("UserPinsLayer")
            {   
                DataSource = new MemoryProvider(features),
                Style = null
            };
        }
        
        public IFeature CreateUserFeature(User user, MapView mv)
        {
            var UserStyle = user.UserIcon;

            var feature = new PointFeature(user.UserPin.Position.ToMapsui());

            feature.Styles.Add(PositionDot());

            feature.Styles.Add(new LabelStyle
            {
                Text = user.UserID,
                HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Center,
                VerticalAlignment = LabelStyle.VerticalAlignmentEnum.Top,
                
            });

            features.Add(feature);
            if (userlayer == null)
                CreateLayer();
            else
            {
                userlayer.DataHasChanged();
                mv.Refresh();
            }

            return feature;
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
            var bitmapId = typeof(CustomLayer).LoadSvgId(embeddedResourcePath);
            return new SymbolStyle { BitmapId = bitmapId, SymbolScale = scale, SymbolOffset = new RelativeOffset(0.0, 0.0) };
        }
        
    }
}