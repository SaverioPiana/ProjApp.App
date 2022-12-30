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

namespace ProjApp.MapEl
{
    public class CustomSymbols
    {
        public static ILayer CreateStylesLayer(MRect envelope)
        {
            List<MPoint> puntiacaso = new List<MPoint>();
            puntiacaso.Add(new Position(41.76887981604152, 12.359517871030286).ToMapsui());
            puntiacaso.Add(new Position(41.770777318054115, 12.365075324403408).ToMapsui());
            puntiacaso.Add(new Position(41.77239216917359, 12.369207327073685).ToMapsui());

            return new MemoryLayer
            {
                Name = "Styles Layer",
                Features = CreateDiverseFeatures(puntiacaso),
                Style = null,
                IsMapInfoLayer = true
            };
        }

        private static IEnumerable<IFeature> CreateDiverseFeatures(IEnumerable<MPoint> randomPoints)
        {
            var features = new List<IFeature>();
            var counter = 0;
            var styles = CreateDiverseStyles().ToList();
            foreach (var point in randomPoints)
            {
                var feature = new PointFeature(point)
                {
                    ["Label"] = counter.ToString()
                };
                feature.Styles.Add(styles[counter]);
                feature.Styles.Add(PositionDot());
                features.Add(feature);
                counter++;
                if (counter == styles.Count) counter = 0;

            }
            features.Add(CreatePointWithStackedStyles());
            return features;
        }

        private static IStyle PositionDot()
        {
            return new SymbolStyle { SymbolScale = 0.2, Fill = new Brush(new Color(40, 40, 40)) };
        }

        private static IEnumerable<IStyle> CreateDiverseStyles()
        {
            const int diameter = 16;
            return new List<IStyle>
            {
                //new SymbolStyle {SymbolScale = 0.8, SymbolOffset = new Offset(0, 0), SymbolType = SymbolType.Rectangle},
                //new SymbolStyle {SymbolScale = 0.6, SymbolOffset = new Offset(diameter, diameter), SymbolType = SymbolType.Rectangle, Fill = new Brush(Color.Red)},
                //new SymbolStyle {SymbolScale = 1, SymbolOffset = new Offset(diameter, -diameter), SymbolType = SymbolType.Rectangle},
                //new SymbolStyle {SymbolScale = 1, SymbolOffset = new Offset(-diameter, -diameter), SymbolType = SymbolType.Rectangle},
                //new SymbolStyle {SymbolScale = 0.8, SymbolOffset = new Offset(0, 0)},
                //new SymbolStyle {SymbolScale = 1.2, SymbolOffset = new Offset(diameter, 0)},
                //new SymbolStyle {SymbolScale = 1, SymbolOffset = new Offset(0, diameter)},
                //new SymbolStyle {SymbolScale = 1, SymbolOffset = new Offset(diameter, diameter)},
                CreateBitmapStyle("Resources.Images.pinicon.png", 0.6),
                CreateBitmapStyle("Resources.Images.pinicon.png", 0.6),
                CreateBitmapStyle("Resources.Images.pinicon.png", 0.6),
                CreateBitmapStyle("Resources.Images.pinicon.png", 0.6),
            };
        }

        private static SymbolStyle CreateBitmapStyle(string embeddedResourcePath, double scale)
        {
            var bitmapId = typeof(CustomSymbols).LoadBitmapId(embeddedResourcePath);
            return new SymbolStyle { BitmapId = bitmapId, SymbolScale = scale, SymbolOffset = new Offset(0, 20) };
        }

        private static SymbolStyle CreateSvgStyle(string embeddedResourcePath, double scale)
        {
            var bitmapId = typeof(CustomSymbols).LoadSvgId(embeddedResourcePath);
            return new SymbolStyle { BitmapId = bitmapId, SymbolScale = scale, SymbolOffset = new RelativeOffset(0.0, 0.0) };
        }

        private static IFeature CreatePointWithStackedStyles()
        {
            var feature = new PointFeature(new Position(41.86887981604152, 12.379517871030286).ToMapsui());

            feature.Styles.Add(new SymbolStyle
            {
                SymbolScale = 2.0f,
                Fill = null,
                Outline = new Pen { Color = Color.Yellow }
            });

            feature.Styles.Add(new SymbolStyle
            {
                SymbolScale = 0.8f,
                Fill = new Brush { Color = Color.Red }
            });

            feature.Styles.Add(new SymbolStyle
            {
                SymbolScale = 0.5f,
                Fill = new Brush { Color = Color.Black }
            });
            
            feature.Styles.Add(new LabelStyle
            {
                Text = "Stacked Styles",
                HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Left
            });

            return feature;
        }
    }
}