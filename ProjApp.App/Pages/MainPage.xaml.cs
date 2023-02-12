using CommunityToolkit.Mvvm.Messaging;
using Mapsui;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.UI.Maui;
using Microsoft.AspNetCore.SignalR.Client;
using ProjApp.Gioco;
using ProjApp.MapEl;
using ProjApp.ViewModel;
using System.Collections.ObjectModel;
#if ANDROID
using static ProjApp.MainActivity;
#endif

namespace ProjApp;

public partial class MainPage : ContentPage
{
    public MainPage(MainPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override bool OnBackButtonPressed()
    {
        return true;
    }

    protected override void OnAppearing()
    {
        (BindingContext as MainPageViewModel).Constructor();
        base.OnAppearing();
        #if ANDROID
        WeakReferenceMessenger.Default.Send(new FullScreenMessage("HideOsNavigationBar"));
        #endif
    }
}

