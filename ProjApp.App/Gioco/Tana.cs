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
using NetTopologySuite.Geometries;
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
        private WritableLayer circleLayer;
        private Geometry cerchio; 
        private const double RADIUS_TANA = 50;

        public Geometry Cerchio { get { return cerchio; } }


        public Tana(MPoint p)
        {
            MPoint punto = new(SphericalMercator.ToLonLat(p));
            position = new(punto.Y, punto.X);  
        }

        //questo costruttore lo usiamo quando creiamo la tana da server
        public Tana(SerializableCoordinate c)
        {
            //qua "c" ci arriva gia in lat lon perche l'admin la crea col
            //costruttore sopra che gia la converte in lat lon
            position = new(c.Y, c.X);
        }

        public void drawArea()
        {
            CreaLayersTana();
            OurMapController.mapView.Map.Layers.Add(circleLayer);
            OurMapController.mapView.Map.Layers.Add(tanalayer);
        }
        public void CreaLayersTana()
        {
            tanalayer = new WritableLayer()
            {
                Style = null
            };

            circleLayer = new WritableLayer()
            {
                Style = null
            };

            CreateTanaFeatures();
        }

        public void CreateTanaFeatures()
        {
            IFeature feature = new PointFeature(position.ToMapsui());

            feature.Styles.Add(CustomLayerExtensions.CreateBitmapStyle("tanaicon.png", 0.55F, new RelativeOffset(0.0, 0.0)));

            tanalayer.Add(feature);

            IFeatureProvider circle = new Circle
            {
                Center = position,
                Radius = new(meters: RADIUS_TANA),
                Quality = 100,
                FillColor = Color.FromRgba("#006F0032"),
                StrokeColor = Colors.Green,
                StrokeWidth = 2,
                MaxVisible = OurMapController.mapView.Map.Resolutions[16]
            };

            circleLayer.Add(circle.Feature);

            cerchio = circle.Feature.Geometry;
        }
    }
}
