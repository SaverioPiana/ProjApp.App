using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjApp.MapEl.Serializable
{
    public class SerializableCoordinate 
    {
        public double X { get; set; }
        public double Y { get; set; }



        public SerializableCoordinate (double x, double y)
        {
            this.X = x;
            this.Y = y;
        }


        public static SerializableCoordinate[] fromCoordinateArray (Coordinate[] coordinates)
        {
            List<SerializableCoordinate> list = new();
            foreach (Coordinate c in coordinates)
            {
                list.Add(new SerializableCoordinate (c.X, c.Y));

            }
            return list.ToArray();
        }

    }
}
