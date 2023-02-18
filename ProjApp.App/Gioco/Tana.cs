using CommunityToolkit.Mvvm.Messaging;
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
using static ProjApp.ViewModel.MainPageViewModel;
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
        public const double RADIUS_TANA = 50;

        public Geometry Cerchio { get { return cerchio; } }


        public Tana(MPoint p)
        {
            MPoint punto = new(SphericalMercator.ToLonLat(p));
            position = new(punto.Y, punto.X);
        }

        //questo costruttore lo usiamo quando creiamo la tana da server
        public Tana(SerializableCoordinate c, MapView mv)
        {
            //qua "c" ci arriva gia in lat lon perche l'admin la crea col
            //costruttore sopra che gia la converte in lat lon
            position = new(c.Y, c.X);

            drawArea(mv);
        }

        public void drawArea(MapView mv)
        {
            CreaLayersTana(mv);
            mv.Map.Layers.Add(circleLayer);
            mv.Map.Layers.Add(tanalayer);
        }
        public void CreaLayersTana(MapView mv)
        {
            tanalayer = new WritableLayer()
            {
                Style = null
            };

            circleLayer = new WritableLayer()
            {
                Style = null
            };

            CreateTanaFeatures(mv);
        }

        public void CreateTanaFeatures(MapView mv)
        {
            IFeature feature = new PointFeature(position.ToMapsui());

            feature.Styles.Add(CustomLayerExtensions.CreateBitmapStyle("tanaicon.png", 0.55F, new RelativeOffset(-0.03, 0.45)));

            tanalayer.Add(feature);

            IFeatureProvider circle = new Circle
            {
                Center = position,
                Radius = new(meters: RADIUS_TANA),
                Quality = 100,
                FillColor = Color.FromRgba("#006F0032"),
                StrokeColor = Colors.Green,
                StrokeWidth = 2,
                MaxVisible = mv.Map.Resolutions[mv.Map.Resolutions.Count()-4]
            };

            circleLayer.Add(circle.Feature);

            cerchio = circle.Feature.Geometry;
        }
    }
}
