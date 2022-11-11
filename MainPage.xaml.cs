
using Mapsui;
using Mapsui.Extensions;
using Mapsui.Projections;

using ProjApp.Map;

namespace ProjApp;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        const double STARTING_RES = 0.1;

        
        InitializeComponent();

        var mapControl = new Mapsui.UI.Maui.MapControl();
        mapControl.Map?.Layers.Add(CartoDBVoyagerTileProviderClass.CreateTileLayer());
        var map = mapControl.Map;

        var centerOfMadonnetta =  new Mapsui.MPoint(12.362896, 41.770261);
        var sugo = SphericalMercator.FromLonLat(centerOfMadonnetta);

        map.Home = n => n.NavigateTo(center: sugo, resolution: STARTING_RES );
        Content = mapControl;
    }
}

