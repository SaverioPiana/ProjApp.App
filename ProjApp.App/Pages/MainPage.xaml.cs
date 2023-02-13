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
        CloseDrawer(); //se lo aspetti non si apre mai la pagina DO NOT AWAIT
        base.OnAppearing();
        #if ANDROID
        WeakReferenceMessenger.Default.Send(new FullScreenMessage("HideOsNavigationBar"));
        #endif
    }

    uint duration = 450;
    double openY = 10;
    bool IsDrawerOpen = false;
    

    async void DrawerDownOrUp(System.Object sender, System.EventArgs e)
    {   
        if(IsDrawerOpen)
        {
            IsDrawerOpen = false;
            await CloseDrawer();
        }
        else
        {
            IsDrawerOpen = true;
            await OpenDrawer();
        }
        
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
                BottomDrawer.TranslationY = openY + e.TotalY;
            }

        }
        else if (e.StatusType == GestureStatus.Completed)
        {
            //Debug.WriteLine($"Completed: {e.TotalY}");
            if (lastPanY < 70)
            {
                IsDrawerOpen= true;
                await OpenDrawer();
            }
            else
            {
                IsDrawerOpen = false;
                await CloseDrawer();
            }
            
        }
    }

    async Task OpenDrawer()
    {
        await Task.WhenAll
        (
            BottomDrawer.TranslateTo(0, openY, length: duration, easing: Easing.CubicInOut),
            BottomDrawerArrow.RotateTo(180, duration, Easing.CubicInOut)
        );
        
    }

    async Task CloseDrawer()
    {
        await Task.WhenAll
        (
            BottomDrawer.TranslateTo(0, 430, length: duration, easing: Easing.CubicInOut),
            BottomDrawerArrow.RotateTo(0, duration, Easing.CubicInOut)
        );
        
    }
}

