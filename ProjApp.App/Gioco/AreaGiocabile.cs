
using Mapsui.Projections;
using Mapsui;
using Mapsui.UI.Maui;
using ProjApp.MapEl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetTopologySuite.Geometries;
using Position = Mapsui.UI.Maui.Position;
using ProjApp.MapEl.Serializable;

namespace ProjApp.Gioco
{
    public class AreaGiocabile
    {
        public List<NetTopologySuite.Geometries.Coordinate> bordi;

        public AreaGiocabile()
        {
            bordi = new List<NetTopologySuite.Geometries.Coordinate>();
            
        }

        public void puntoBordo(MPoint worldPosition)
        {
            Position position;

            MPoint punto = new(SphericalMercator.ToLonLat(worldPosition));
            position = new(punto.Y, punto.X);

            OurMapController.AddPinFromFile(position, "Bordi", "reddot.png", 0.15F);

            bordi.Add(position.ToCoordinate());

        }

        public void creaArea()
        {
            List<Pin> pins = new List<Pin>(OurMapController.mapView.Pins);
                foreach (Pin pin in pins)
                {
                    if (pin.Label == "Bordi")
                    {
                        OurMapController.mapView.Pins.Remove(pin);
                    }
                }
            bordi.Add(bordi.First());

            drawArea(bordi.ToArray());
        }

        public void drawArea(Coordinate[] c)
        {

            OurMapController.mapView.Map.Layers.Add(OurMapController.CreateCustomLayer("AreaDiGioco", c));

        }

        public void drawArea(SerializableCoordinate[] c)
        {
            List <Coordinate> lc = new();

            foreach (SerializableCoordinate c2 in c)
            {
                lc.Add(new Coordinate(c2.X, c2.Y));
            }

            OurMapController.mapView.Map.Layers.Add(OurMapController.CreateCustomLayer("AreaDiGioco", lc.ToArray()));

        }


    }
}
