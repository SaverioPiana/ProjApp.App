using Android.Text.Style;
using Mapsui.UI.Maui;
using Microsoft.AspNetCore.SignalR.Client;
using ProjApp.Gioco;
using System.Text.Json.Serialization;
using System.Text.Json;
using Mapsui.Nts;
using Mapsui.Extensions;

namespace ProjApp.MapEl.GPS
{
    public class MyUser
    {
        private CancellationTokenSource _cancelTokenSource;
        private bool _isCheckingLocation;
        public static User user;

        private static bool want_sendposition = true;

        //IL NICKNAME DOVRA METTERLO L UTENTE CON UNA BOX
        public MyUser(MapView mv)
        {
            user = new("Nickname", DeviceInfo.Name, new(), mv);
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

                    GeolocationRequest request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(6));

                    _cancelTokenSource = new CancellationTokenSource();

                    Location location = await Geolocation.Default.GetLocationAsync(request, _cancelTokenSource.Token);


                    if (location != null)
                    {
                         user.UserPin.Position = new Position(location.Latitude, location.Longitude);
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

        //non funziona
        public static async void inviaPosSignalR(HubConnection connection_nelMC, int delay)
        {
            while (want_sendposition)
            {
                if (connection_nelMC.State.Equals(HubConnectionState.Connected))
                {
                    string jsonUser = JsonSerializer.Serialize<User>(user,
                          new JsonSerializerOptions
                          {
                              NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
                              IgnoreReadOnlyProperties = true,
                              IgnoreReadOnlyFields = true
                          });

                    await connection_nelMC.InvokeAsync("SendPosition",
                          arg1: jsonUser);
                }
                await Task.Delay(delay);
            }

        }
    }
}
