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
        //costanti
        public const double DISTANZA_AVVISO = 40;
        public const double DISTANZA_INSEGUIMENTO = 20;
        public const double DISTANZA_CATTURA = 5;

        public const int DELAY_INIZIO_GIOCO = 180000;

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

        //LOGICA EVENTI DISTANZA E VISIBILITA PIN
        public static async Task<bool> ShouldPinBeVisible(bool isPinCercatore, bool previousVal, double distanceMts, bool IsHuntPossible)
        {
            bool res = previousVal;

            //stesso ruolo
            if ((isPinCercatore && MyUser.user.IsCercatore) || (!isPinCercatore && !MyUser.user.IsCercatore))
            {
                res = true;
            }
            //ruoli diversi
            if (!isPinCercatore && MyUser.user.IsCercatore)
            {
                if (IsHuntPossible)
                {
                    res = await EventOnDistance(distanceMts, previousVal);
                }
                else return false;
            }
            if (isPinCercatore && !MyUser.user.IsCercatore)
            {
                if (IsHuntPossible)
                {
                    res = await EventOnDistance(distanceMts, previousVal);
                }
                else return false;
            }

            return res;
        }

        //DA FARE
        public static async Task<bool> EventOnDistance(double distanceMts, bool previousVal)
        {
            bool res = previousVal;

            //avviso solo la prima volta
            if (distanceMts <= DISTANZA_AVVISO)
            {
                //SAS AVVISO SILENZIOSOS
                if (true) //prima volta o timer scaduto -> avviso
                {

                }
                //e in ogni caso -> 
                res = false;
            }
            //la prima volta avviso e vibrazione(a ogni cooldown del timer), poi solo visibilita true del pin
            if (distanceMts <= DISTANZA_INSEGUIMENTO)
            {
                //SAS INSEGUIMENTO PAZZO
                if (true) //prima volta o timer scaduto -> avviso
                {

                }
                //e in ogni caso -> 
                res = true;
            }
            //solo una volta, evento cattura
            if (distanceMts <= DISTANZA_CATTURA)
            {
                //SAS PRESOS
                if (true) //se non è gia stato preso
                {
                    //prendilo
                }
            }

            return res;
        }

    }
}
