using Mapsui.Projections;
using Mapsui;
using Mapsui.UI.Maui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjApp.Map.GPS
{
    public class MyPosition
    {

        public Position position { get; set; }

        public MyPosition()
        {
            position = Get_Position().Result;
        }


        public async Task<Position> Get_Position()
        {
            try
            {
                var location = await Geolocation.GetLastKnownLocationAsync();
                if (location == null)
                {
                    location = await Geolocation.GetLocationAsync(new GeolocationRequest
                    {
                        DesiredAccuracy = GeolocationAccuracy.Best,
                        Timeout = TimeSpan.FromSeconds(20)
                    });
                }  
                return new Position(location.Latitude, location.Longitude);
            }
            catch (Exception ex)
            {
                // CODICE PERICOLOSO !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                Console.WriteLine($"Error: {ex.Message}");
                return new Position(0,0);
            }
        }
    }
}
