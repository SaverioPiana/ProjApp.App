using Mapsui;
using Mapsui.Nts.Extensions;
using Mapsui.Projections;
using NetTopologySuite.Geometries;
using ProjApp.MapEl.GPS;
using Location = Microsoft.Maui.Devices.Sensors.Location;
using Point = NetTopologySuite.Geometries.Point;

namespace ProjApp.Gioco
{
    public static class GameLogic
    {
        public static async Task<List<User>> whoOutsideTheArea()
        {
            List<Coordinate> bordi = MyUser.currPartita.area.bordi;
            //bordi.Remove(bordi.First());
            //ma quante funz su liste tipo PF WOW!(ci sta pure Any che e' tipo exists)
            //List<User> UO = (List<User>)MyUser.currPartita.Players.Where( x => IsOutsideTheArea(bordi, x.Position));
            List<User> UO = new List<User>();
            foreach (User p in MyUser.currPartita.Players)
            {
                if (!IsInsideTheArea(bordi, p.Position))
                    UO.Add(p);
            }
            return UO;
        }

        // uso di "Ray Casting Method" , chat GPT in aiuto
        public static bool IsInsideTheArea(List<Coordinate> polygon, Location testPoint)
        {  
            MPoint p = SphericalMercator.FromLonLat(new(testPoint.Longitude,testPoint.Latitude));
            return IsInsideTheArea(polygon, p); ;
        }

        public static bool IsInsideTheArea(List<Coordinate> polygon, MPoint testPoint)
        {
            Coordinate c = testPoint.ToCoordinate();
            Point p = new(c);
            var area = new Polygon(new(polygon.ToArray()));
            return area.Contains(p); ;
        }

        public static bool IsInsideTheArea(List<Coordinate> polygon, Geometry polygon2)
        {
            var area = new Polygon(new(polygon.ToArray()));
            return area.Contains(polygon2); 
        }
    }
}
