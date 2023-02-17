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
using Mapsui.ViewportAnimations;
using ProjApp.MapEl.GPS;
using CommunityToolkit.Mvvm.Messaging.Messages;
using ProjApp.Messagges;
#if ANDROID
using static ProjApp.MainActivity;
#endif

namespace ProjApp;

public partial class MainPage : ContentPage
{

    public const string INFO_PARTITA_TEXT_DEFAULT = "Informazioni partita";
    public const string INFO_PARTITA_TEXT_AVVISO = "Avviso";

    public const string AVVISO_NOTIFICA = "VibrazioneSingola";
    public const string AVVISO_INSEGUIMENTO = "VibrazioniAnsiose";

    public static CancellationTokenSource _cancelTokenSourceAvviso = null;

    public MainPage(MainPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;

        //registro la pagina per i messaggi con il viewmodel
        //BAD PRACTICE -----> MA DOBBIAMO CONSEGNARE, NON C'E' TEMPO
        WeakReferenceMessenger.Default.Register<OpenAvvisoMessage>(this, async(r, m) =>
        {
            try
            {
                //solo un avviso per volta
                if (_cancelTokenSourceAvviso != null)
                {
                    _cancelTokenSourceAvviso.Cancel();
                }
                _cancelTokenSourceAvviso = new();
                TimeSpan vibrationLength = TimeSpan.Zero;
                int numeroVibr = 0;
                (BindingContext as MainPageViewModel).TendinaText = INFO_PARTITA_TEXT_AVVISO;
                _ = Task.Run(async () =>
                {
                    switch (m.Value.EventType)
                    {
                        case (AVVISO_NOTIFICA):
                            vibrationLength = TimeSpan.FromSeconds(1);
                            Vibration.Default.Vibrate(vibrationLength);
                            break;
                        case (AVVISO_INSEGUIMENTO):
                            numeroVibr = 4;
                            for (int i = 0; i < 4; i++)
                            {
                                Vibration.Default.Vibrate();
                                await Task.Delay(2500, _cancelTokenSourceAvviso.Token);

                                if (_cancelTokenSourceAvviso.IsCancellationRequested)
                                {
                                    return;
                                }
                            }
                            break;
                    }
                });

                await OpenDrawer(m.Value.EventParameter, _cancelTokenSourceAvviso.Token);

                await Task.Delay(15000, _cancelTokenSourceAvviso.Token);

                if (!_cancelTokenSourceAvviso.IsCancellationRequested)
                {
                    await CloseDrawer();
                    (BindingContext as MainPageViewModel).TendinaText = INFO_PARTITA_TEXT_DEFAULT;
                }
            } catch(TaskCanceledException ex) { //BELLO QUI PALESE ESPLODE TUTTO RIP
                                              }
        });
    }

    protected override bool OnBackButtonPressed()
    {
        if (IsDrawerOpen)
        {
            CloseDrawer();
            IsDrawerOpen= false;
        }
        return true;
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

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        if (IsDrawerOpen)
        {
            CloseDrawer();
            IsDrawerOpen= false;
        }
    }

    //messaggio per aprire la tendina per gli avvisi generati ad eventi per la distanza tra giocatori
    public class OpenAvvisoMessage : ValueChangedMessage<UI_Event<double>>
    {
        public OpenAvvisoMessage(UI_Event<double> value) : base(value)
        {
        }
    }
    //gestione messaggio
    async Task OpenDrawer(double customOpenY, CancellationToken tk)
    {
        if(tk.IsCancellationRequested) return;
        await Task.WhenAll
        (
            BottomDrawer.TranslateTo(0, customOpenY, length: duration, easing: Easing.CubicInOut),
            BottomDrawerArrow.RotateTo(0, duration, Easing.CubicInOut)
        );
        IsDrawerOpen = true;
    }

    uint duration = 450;
    double openY = 100;
    bool IsDrawerOpen = false;
    

    async void DrawerDownOrUp(System.Object sender, System.EventArgs e)
    {   
        if(IsDrawerOpen)
        {
            await CloseDrawer();
        }
        else
        {
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
            BottomDrawer.TranslateTo(0, openY, length: duration, easing: Easing.CubicInOut),
            BottomDrawerArrow.RotateTo(0, duration, Easing.CubicInOut)
        );
        IsDrawerOpen= true;
    }

    async Task CloseDrawer()
    {
        await Task.WhenAll
        (
            BottomDrawer.TranslateTo(0, 430, length: duration, easing: Easing.CubicInOut),
            BottomDrawerArrow.RotateTo(180, duration, Easing.CubicInOut)
        );
        IsDrawerOpen= false;
    }
}

