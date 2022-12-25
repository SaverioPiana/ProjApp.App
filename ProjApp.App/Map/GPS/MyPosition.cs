using Mapsui.UI.Maui;

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


                    if (location != null)
                    {
                         position = new Position(location.Latitude, location.Longitude);
                         Console.WriteLine($"Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude: {location.Altitude}");
                    }

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
