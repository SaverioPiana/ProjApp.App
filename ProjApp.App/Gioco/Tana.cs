using Mapsui;
using Mapsui.Nts.Extensions;
using Mapsui.Projections;
using Mapsui.UI.Maui;
using ProjApp.MapEl;
using ProjApp.MapEl.Serializable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjApp.Gioco
{
    public class Tana
    {
        public Position position;

        public Tana(MPoint p)
        {
            MPoint punto = new(SphericalMercator.ToLonLat(p));
            position = new(punto.Y, punto.X);   

            OurMapController.AddPin(position, "Tana" , Colors.Blue);
        }

        public Tana(SerializableCoordinate c)
        {
            MPoint pun = new(c.X, c.Y);
            MPoint corr = new(SphericalMercator.ToLonLat(pun));
            position = new(corr.Y, corr.X);

            OurMapController.AddPin(position, "Tana", Colors.Blue);
        }
    }
}
