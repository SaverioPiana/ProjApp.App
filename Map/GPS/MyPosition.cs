using Mapsui.Projections;
using Mapsui;
using Mapsui.UI.Maui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kotlin.Contracts;

namespace ProjApp.Map.GPS
{
    public class MyPosition
    {
        private CancellationTokenSource _cancelTokenSource;
        private bool _isCheckingLocation;
        public static Position position { get; set; }

        public MyPosition()
        {

            position = new();
                
        }

        /* NON VEDO A CHE SERVE SAVE SPIEGA */
        //public async Task<Position> returnPosition()
        //{
        //    await Get_Position();
        //    return position;
        //}

        public async Task Get_Position()
        {
            try
                {
                    _isCheckingLocation = true;

                    GeolocationRequest request = new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(10));

                    _cancelTokenSource = new CancellationTokenSource();

                    Location location = await Geolocation.Default.GetLocationAsync(request, _cancelTokenSource.Token);
                    position = new Position(location.Latitude, location.Longitude);


                    if (location != null)
                        Console.WriteLine($"Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude: {location.Altitude}");

                }
                // Catch one of the following exceptions:
                //   FeatureNotSupportedException
                //   FeatureNotEnabledException
                //   PermissionException
                catch (Exception ex)
                {
                    Console.WriteLine($"EXCEPTION position not found {ex.Message}");

                }
                finally
                {
                    _isCheckingLocation = false;
                }
            
        }
        public void CancelRequest()
        {
            if (_isCheckingLocation && _cancelTokenSource != null && _cancelTokenSource.IsCancellationRequested == false)
                _cancelTokenSource.Cancel();
        }
    }
}
