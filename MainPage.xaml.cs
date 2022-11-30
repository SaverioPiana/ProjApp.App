using Mapsui;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.UI.Maui;
using Microsoft.AspNetCore.SignalR.Client;
using ProjApp.Map;
using System.Collections.ObjectModel;

namespace ProjApp;

public  partial class MainPage : ContentPage
{
    //variabile di connessione all HUB di SignalR
    private readonly HubConnection _connection;
    public MainPage()
    {
        InitializeComponent();

        MainThread.BeginInvokeOnMainThread(() =>
        {
            CheckANDSetPermission();
        });
    //Connession a SignalR
        _connection = new HubConnectionBuilder()
            .WithUrl("/lobby")
            .WithAutomaticReconnect()
            .Build();

    //Quando ricevo una posizione broadcast la stampo su console per debugging
        _connection.On<string, double, double>("PositionReceived", (user, lat ,lon) =>
        {
            Console.WriteLine($"/////////Posizione ricevuta da:{user} , lat:{lat}, lon: {lon}");
        });
    //starto la connessione
        Task.Run(() =>
        {
            Dispatcher.Dispatch(async () =>
            await _connection.StartAsync());
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
    
    

}

