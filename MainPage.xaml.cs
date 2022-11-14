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
    public MainPage()
    {
        InitializeComponent();

        Content = OurMapController.MapInitializer();
    }
}

