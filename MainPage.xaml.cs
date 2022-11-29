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
        //MainThread.BeginInvokeOnMainThread(async() =>
        //{
        //    var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

        //    if (status != PermissionStatus.Granted)
        //        await Task.Run(()=> Permissions.RequestAsync<Permissions.LocationWhenInUse>());
        //});

         
           Content = new OurMapController().MapInitializer();
           

    }
    
    

}

