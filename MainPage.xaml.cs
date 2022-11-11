
using Mapsui;
using Mapsui.Extensions;
using Mapsui.Projections;

using ProjApp.Map;

namespace ProjApp;

public partial class MainPage : ContentPage
{
    public Location posizione = null;

    private CancellationTokenSource _cancelTokenSource;
    private bool _isCheckingLocation;

    public async Task GetCurrentLocation()
    {
        try
        {
            _isCheckingLocation = true;

            GeolocationRequest request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));

            _cancelTokenSource = new CancellationTokenSource();

            Location location = await Geolocation.Default.GetLocationAsync(request, _cancelTokenSource.Token);

            if (location != null)
                Console.WriteLine($"Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude: {location.Altitude}");
        }
        // Catch one of the following exceptions:
        //   FeatureNotSupportedException
        //   FeatureNotEnabledException
        //   PermissionException
        catch (Exception ex)
        {
            // Unable to get location
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



    public MainPage()
    {
        const double STARTING_RES = 0.1;

        
        InitializeComponent();

        var mapControl = new Mapsui.UI.Maui.MapControl();
        mapControl.Map?.Layers.Add(CartoDBVoyagerTileProviderClass.CreateTileLayer());
        var map = mapControl.Map;
       
        GetCurrentLocation();
        
        var centerOfMadonnetta =  new Mapsui.MPoint(posizione.Longitude, posizione.Latitude);
        var sugo = SphericalMercator.FromLonLat(centerOfMadonnetta);
        map.Home = n => n.NavigateTo(center: sugo, resolution: STARTING_RES );
        Content = mapControl;
    }
}

