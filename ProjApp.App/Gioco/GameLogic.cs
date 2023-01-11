using NetTopologySuite.Geometries;
using ProjApp.MapEl.GPS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Location = Microsoft.Maui.Devices.Sensors.Location;

namespace ProjApp.Gioco
{
    public static class GameLogic
    {
        //dichiarazione evento
        public static event EventHandler<List<User>> UsersOutside;

        public static void whoOutsideTheArea()
        {
            List<Coordinate> bordi = MyUser.currPartita.area.bordi;
            bordi.Remove(bordi.First());
            //ma quante funz su liste tipo PF WOW!(ci sta pure Any che e' tipo exists)
            //List<User> UO = (List<User>)MyUser.currPartita.Players.Where( x => IsOutsideTheArea(bordi, x.Position));
            List<User> UO = new List<User>();
            foreach (User p in MyUser.currPartita.Players)
            {
                if (!IsInsideTheArea(bordi, p.Position))
                    UO.Add(p);
            }

            if (UO.Count > 0)
            {
                //evento
                OnUserOutside(UO);
            }
        }
         public static void OnUserOutside(List<User> e)
        {
            UsersOutside?.Invoke(new(), e);
        }

        // uso di "Ray Casting Method" , chat GPT in aiuto
        private static bool IsInsideTheArea(List<Coordinate> bordi, Location point)
        {
            int i, j;
            bool res = false;
            int nvert = bordi.Count;
            for (i = 0, j = nvert - 1; i < nvert; j = i++)
            {
                if (((bordi[i].Y > point.Latitude) != (bordi[j].Y > point.Latitude)) &&
             (point.Longitude < (bordi[j].X - bordi[i].X) * (point.Latitude - bordi[i].Y) / (bordi[j].Y - bordi[i].Y) + bordi[i].X))

                    //The function works by casting a horizontal ray from the point being tested to the right,
                    //and then counting the number of times this ray intersects with the edges of the polygon.
                    //If the number of intersections is odd, then the point is inside the polygon;
                    //if the number of intersections is even, then the point is outside the polygon.

                    //DA VEDERE (perche aggiungo il primo elemento all fine in creaArea di AreaGiocabile
                    //=> The function does assume that the vertex is ordered and that the last vertex is connected to the first one
                    res = !res;
            }
            return res;
        }
    
    }
}
