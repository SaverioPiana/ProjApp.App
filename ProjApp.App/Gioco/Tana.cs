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

        //questo costruttore lo usiamo quando creiamo la tana da server
        public Tana(SerializableCoordinate c)
        {
            //qua "c" ci arriva gia in lat lon perche l'admin la crea col
            //costruttore sopra che gia la converte in lat lon
            position = new(c.Y, c.X);
            OurMapController.AddPin(position, "Tana", Colors.Blue);
        }
    }
}
