
using Android.OS;
using Mapsui;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.UI.Maui;
using ProjApp.Map;
using System.Collections.ObjectModel;

namespace ProjApp;

public  partial class MainPage : ContentPage
{

    public async Task<Location> Get_Location()
    {
        try
        {
             var location = await Geolocation.GetLastKnownLocationAsync();
            if(location == null)
            {
                 location = await Geolocation.GetLocationAsync(new GeolocationRequest
                {
                    DesiredAccuracy = GeolocationAccuracy.Best,
                    Timeout = TimeSpan.FromSeconds(20)
                }) ;
            }
            return location;

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}" );
            return null;
        }
    }
    public Location MapHomeToCurrentLocation(Mapsui.Map map)
    {
        const double STARTING_RES = 0.1;
        Location posizione = null;
        MPoint mpointPosizione = new Mapsui.MPoint(0, 0);

        try
        {
            posizione = Get_Location().Result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Errore nel mainPage: {ex.Message}");
        }
        if (posizione != null)
        {
            mpointPosizione.X = posizione.Longitude;
            mpointPosizione.Y = posizione.Latitude;
        }
        MPoint punto = SphericalMercator.FromLonLat(mpointPosizione);
        map.Home = n => n.NavigateTo(center: punto,
                                     resolution: STARTING_RES);
        return posizione;
    }



    public MemoryLayer CreaPunto(Mapsui.Map map , MPoint posizione)
    {
       
        var pointFeature = new PointFeature(posizione.X, posizione.Y);
        pointFeature["name"] = "currentLocation";
        Collection<IEnumerable<IFeature>> features = new();
        features.Add((IEnumerable<IFeature>)pointFeature);



        return new MemoryLayer
        {
            Name = "punti",
            IsMapInfoLayer = true,
            Features = (IEnumerable<IFeature>)features
        };

    }





    public MainPage()
    {
        InitializeComponent();

        var mapControl = new Mapsui.UI.Maui.MapControl();
        mapControl.Map?.Layers.Add(CartoDBVoyagerTileProviderClass.CreateTileLayer());
        var map = mapControl.Map;
        Location position = MapHomeToCurrentLocation(map);
        MapView mapView = new();
        mapView.Map = map;
        mapView.MyLocationLayer.UpdateMyLocation(new Position(position.Latitude, position.Longitude), true);
        Content = mapView;
    }
}

