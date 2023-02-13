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
using System.Diagnostics;
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

    protected override void OnAppearing()
    {
        (BindingContext as MainPageViewModel).Constructor();
        base.OnAppearing();
        #if ANDROID
        WeakReferenceMessenger.Default.Send(new FullScreenMessage("HideOsNavigationBar"));
        #endif
    }

    uint duration = 300;
    double openY = 0;
    

    async void DrawerUp(System.Object sender, System.EventArgs e)
    {
        await OpenDrawer();
    }

    async void DrawerDown(System.Object sender, System.EventArgs e)
    {
        await CloseDrawer();
    }



    double lastPanY = 0;
    
    async void PanGestureRecognizer_PanUpdated(System.Object sender, PanUpdatedEventArgs e)
    {
        if (e.StatusType == GestureStatus.Running)
        {
            
            lastPanY = e.TotalY;
            Debug.WriteLine($"Running: {e.TotalY}");
            if (e.TotalY > 0)
            {
                BottomToolbar.TranslationY = openY + e.TotalY;
            }

        }
        else if (e.StatusType == GestureStatus.Completed)
        {
            //Debug.WriteLine($"Completed: {e.TotalY}");
            if (lastPanY < 70)
            {
                await OpenDrawer();
            }
            else
            {
                await CloseDrawer();
            }
            
        }
    }

    async Task OpenDrawer()
    {
        await Task.WhenAll
        (
            BottomToolbar.TranslateTo(0, openY, length: duration, easing: Easing.CubicInOut)
        );
        
    }

    async Task CloseDrawer()
    {
        await Task.WhenAll
        (
            
            BottomToolbar.TranslateTo(0, 430, length: duration, easing: Easing.CubicInOut)
        );
        
    }

    private void Button_Clicked_1(object sender, EventArgs e)
    {

    }
}

