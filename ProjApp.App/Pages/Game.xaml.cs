using CommunityToolkit.Mvvm.Messaging;
using Mapsui;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.UI.Maui;
using Microsoft.AspNetCore.SignalR.Client;
using ProjApp.Gioco;
using ProjApp.MapEl;
using System.Collections.ObjectModel;
using static ProjApp.MainActivity;

namespace ProjApp.Pages;

public  partial class Game : ContentPage
{
    //serve ad entrare in fullscreen
    protected override void OnAppearing()
    {
        base.OnAppearing();
        WeakReferenceMessenger.Default.Send(new FullScreenMessage("HideOsNavigationBar"));
    }


    public Game()
    {
        InitializeComponent();

        //runniamo il check dei permessi sul main thread
        MainThread.BeginInvokeOnMainThread(() =>
        {
            CheckANDSetPermission();
        });

        //Signalr
        Connessione c = new();

        Task.Run(() =>
        {
            Dispatcher.Dispatch(async () =>
            await Connessione.con.StartAsync());
        });

        new OurMapController().MapInitializer();
        Content = OurMapController.mapView;
    }

    //forse va messa una condizione in modo tale che non runni sempre all avvio, tipo salvarci un bool su un file boh
    private async void CheckANDSetPermission()
    {
        var status =  await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

        if (status == PermissionStatus.Denied) //chiamiamo Geolocation perche fa la richiesta del gps 
             await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Lowest));
    }
}

