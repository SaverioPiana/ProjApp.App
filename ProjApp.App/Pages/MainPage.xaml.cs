using CommunityToolkit.Mvvm.Messaging;
using Mapsui;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.UI.Maui;
using Microsoft.AspNetCore.SignalR.Client;
using ProjApp.MapEl;
using System.Collections.ObjectModel;
using static ProjApp.MainActivity;

namespace ProjApp;

public  partial class MainPage : ContentPage
{
    //variabile di connessione all HUB di SignalR
    private readonly HubConnection _connection;
    public MainPage()
    {
        InitializeComponent();

        //runniamo il check dei permessi sul main thread
        MainThread.BeginInvokeOnMainThread(() =>
        {
            CheckANDSetPermission();
        });

       

        //mando la connessione a mapInitializer
        Content = new OurMapController().MapInitializer(_connection);

    }

    //forse va messa una condizione in modo tale che non runni sempre all avvio, tipo salvarci un bool su un file boh
    private async void CheckANDSetPermission()
    {
        var status =  await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

        if (status == PermissionStatus.Denied) //chiamiamo Geolocation perche fa la richiesta del gps 
             await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Lowest));
    }

    protected override bool OnBackButtonPressed()
    {
        return true;
    }

    #if ANDROID
    protected override void OnAppearing()
    {
        base.OnAppearing();
        WeakReferenceMessenger.Default.Send(new FullScreenMessage("HideOsNavigationBar"));
    }
    #endif
}

