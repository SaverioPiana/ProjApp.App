using Android.Icu.Number;
using HarfBuzzSharp;
using Mapsui;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Projections;
using Mapsui.Styles;
using Mapsui.UI.Maui;
using Mapsui.UI.Objects;
using ProjApp.MapEl;
using ProjApp.MapEl.Serializable;
using Color = Microsoft.Maui.Graphics.Color;
using Font = Mapsui.Styles.Font;
using Polygon = NetTopologySuite.Geometries.Polygon;
using Position = Mapsui.UI.Maui.Position;

namespace ProjApp.Gioco
{
    public class Tana
    {
        public Position position;
        private WritableLayer tanalayer;
        private const double RADIUS_TANA = 50;

        public Tana(MPoint p)
        {
            MPoint punto = new(SphericalMercator.ToLonLat(p));
            position = new(punto.Y, punto.X);

            CreaLayerTana();

            OurMapController.mapView.Map.Layers.Add(tanalayer);
        }

        //questo costruttore lo usiamo quando creiamo la tana da server
        public Tana(SerializableCoordinate c)
        {
            //qua "c" ci arriva gia in lat lon perche l'admin la crea col
            //costruttore sopra che gia la converte in lat lon
            position = new(c.Y, c.X);

            CreaLayerTana();

            OurMapController.mapView.Map.Layers.Add(tanalayer);
        }

        public void CreaLayerTana()
        {
            tanalayer = new WritableLayer()
            {
                Style = null
            };
            tanalayer.AddRange(CreateTanaFeatures());
        }

        public IEnumerable<IFeature> CreateTanaFeatures()
        {
            List<IFeature> features = new();

            IFeature feature = new PointFeature(position.ToMapsui());


            feature.Styles.Add(CustomLayer.CreateBitmapStyle("tanaicon.png", 0.55F, new RelativeOffset(0.0, 0.0)));

            feature.Styles.Add(new LabelStyle
            {
                Text = "TANA",
                HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Center,
                VerticalAlignment = LabelStyle.VerticalAlignmentEnum.Top,
                Offset = new RelativeOffset(0.0, -4.0),
                Enabled = true,
                Font = new Font { Size = 13, FontFamily = "Arial" },

                ForeColor = Mapsui.Styles.Color.Green,
                BackColor = new Mapsui.Styles.Brush(Mapsui.Styles.Color.White),
            });

            IFeatureProvider circle = new Circle
            {
                Center = position,
                Radius = new(meters: RADIUS_TANA),
                Quality = 100,
                FillColor = Color.FromRgba("#006F0032"),
                StrokeColor = Colors.DarkGreen,
                StrokeWidth = 2,
                MaxVisible = OurMapController.mapView.Map.Resolutions[16]
            };

            features.Add(feature);
            features.Add(circle.Feature);

            return features;
        }
    }
}
