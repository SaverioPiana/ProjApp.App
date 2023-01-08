using Android.Icu.Number;
using HarfBuzzSharp;
using Mapsui;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Nts.Extensions;
using Mapsui.Projections;
using Mapsui.Styles;
using Mapsui.UI.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using ProjApp.MapEl;
using ProjApp.MapEl.Serializable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Font = Mapsui.Styles.Font;

namespace ProjApp.Gioco
{
    public class Tana
    {
        public Position position;
        private WritableLayer tanalayer;

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
            tanalayer = new WritableLayer();
            tanalayer.AddRange(CreateTanaFeatures());
        }

        public IEnumerable<IFeature> CreateTanaFeatures()
        {
            List<IFeature> features = new();

            IFeature feature = new PointFeature(position.ToMapsui());

            feature.Styles.Add(new SymbolStyle
            {
                SymbolType = SymbolType.Ellipse,
                SymbolScale = 2,
                Outline = new Pen
                {
                    Color = Mapsui.Styles.Color.Green,
                    Width = 2,
                    PenStyle = PenStyle.ShortDashDot,
                    PenStrokeCap = PenStrokeCap.Round
                },
                Fill = new Mapsui.Styles.Brush(new Mapsui.Styles.Color(0,150,0,100)),
                
                

            });

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

            

            features.Add(feature);

            return features;
        }
    }
}
