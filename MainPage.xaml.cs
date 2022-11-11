using ProjApp.Map;

namespace ProjApp;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();

        var mapControl = new Mapsui.UI.Maui.MapControl();
        mapControl.Map?.Layers.Add(CartoDBVoyagerTileProviderClass.CreateTileLayer());
        Content = mapControl;
    }
}

