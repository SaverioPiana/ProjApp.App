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
    double openY = 60;

    async void Button_Clicked(System.Object sender, System.EventArgs e)
    {
        if (Backdrop.Opacity == 0)
        {
            BottomToolbar.IsVisible = true;
            await OpenDrawer();
        }
        else
        {
            await CloseDrawer();
            BottomToolbar.IsVisible = false;
        }
    }

    async void TapGestureRecognizer_Tapped(System.Object sender, System.EventArgs e)
    {
        if (isBackdropTapEnabled)
        {
            await CloseDrawer();
        }
    }

    double lastPanY = 0;
    bool isBackdropTapEnabled = true;
    async void PanGestureRecognizer_PanUpdated(System.Object sender, PanUpdatedEventArgs e)
    {
        if (e.StatusType == GestureStatus.Running)
        {
            isBackdropTapEnabled = false;
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
            isBackdropTapEnabled = true;
        }
    }

    async Task OpenDrawer()
    {
        await Task.WhenAll
        (
            Backdrop.FadeTo(1, length: duration),
            BottomToolbar.TranslateTo(0, openY, length: duration, easing: Easing.CubicInOut)
        );
        Backdrop.IsVisible= true;
    }

    async Task CloseDrawer()
    {
        await Task.WhenAll
        (
            Backdrop.FadeTo(0, length: duration),
            BottomToolbar.TranslateTo(0, 260, length: duration, easing: Easing.CubicInOut)
        );
        Backdrop.IsVisible = false;
    }
}

