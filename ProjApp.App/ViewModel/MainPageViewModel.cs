using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Mapsui.UI.Maui;
using ProjApp.MapEl;
using ProjApp.MapEl.GPS;
using static ProjApp.ViewModel.StartPageViewModel;

namespace ProjApp.ViewModel
{
    public partial class MainPageViewModel : ObservableObject
    {
        [ObservableProperty]
        private MapView contentMapView;
        public MainPageViewModel()
        {
            //runniamo il check dei permessi sul main thread
            MainThread.BeginInvokeOnMainThread(() =>
            {
                CheckANDSetPermission();
            });

            new OurMapController().MapInitializer();
            ContentMapView = OurMapController.mapView;
        }

        //forse va messa una condizione in modo tale che non runni sempre all avvio, tipo salvarci un bool su un file boh
        private async void CheckANDSetPermission()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

            if (status == PermissionStatus.Denied) //chiamiamo Geolocation perche fa la richiesta del gps 
                await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Lowest));
        }

        [RelayCommand]
        public void AbbandonaPartita()
        {
            MyUser.currPartita.LeaveLobby();
            WeakReferenceMessenger.Default.Send<UIChangeAlertStartPage>(new("lobbyHasBeenDeleted", "noPar"));
            Shell.Current.GoToAsync($"//{nameof(StartPage)}");
        }
    }
}