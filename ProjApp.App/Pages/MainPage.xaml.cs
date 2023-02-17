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

    //detail text cacciatori
    public const string TEXTDETAIL_NOTIFICA_SEEKER = "Qualcuno si nasconde nei dintorni";
    public const string TEXTDETAIL_INSEGUIMENTO_SEEKER = "SEI VICINO, CATTURALO!";
    public const string TEXTDETAIL_CATTURA_SEEKER = "Cattura completata! :)";

    //detail text hiders
    public const string TEXTDETAIL_NOTIFICA_HIDER = "Un cacciatore è nei paraggi";
    public const string TEXTDETAIL_INSEGUIMENTO_HIDER = "SCAPPA DAL CACCIATORE!";
    public const string TEXTDETAIL_CATTURA_HIDER = "Sei stato catturato! :(";

    //avvisi
    public const string AVVISO_NOTIFICA = "EventoNotifica";
    public const string AVVISO_INSEGUIMENTO = "EventoInseguimento";
    public const string AVVISO_CATTURA = "EventoCattura";

    public static CancellationTokenSource _cancelTokenSourceAvviso = null;

    public MainPage(MainPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
       
        Brush originalStroke = InfoPartBorder.Stroke;

        //registro la pagina per i messaggi con il viewmodel
        //BAD PRACTICE -----> MA DOBBIAMO CONSEGNARE, NON C'E' TEMPO
        WeakReferenceMessenger.Default.Register<OpenAvvisoMessage>(this, async(r, m) =>
        {
            try
            {
                //cambio colore
                InfoPartBorder.Stroke = Brush.IndianRed;

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
                            if(MyUser.user.IsCercatore)
                            {
                                (BindingContext as MainPageViewModel).TendinaTextDetail = TEXTDETAIL_NOTIFICA_SEEKER;
                            } 
                            else (BindingContext as MainPageViewModel).TendinaTextDetail = TEXTDETAIL_NOTIFICA_HIDER;

                            vibrationLength = TimeSpan.FromSeconds(1);
                            Vibration.Default.Vibrate(vibrationLength);
                            break;
                        case (AVVISO_INSEGUIMENTO):
                            if (MyUser.user.IsCercatore)
                            {
                                (BindingContext as MainPageViewModel).TendinaTextDetail = TEXTDETAIL_INSEGUIMENTO_SEEKER;
                            }
                            else (BindingContext as MainPageViewModel).TendinaTextDetail = TEXTDETAIL_INSEGUIMENTO_HIDER;

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
                        case (AVVISO_CATTURA):
                            if (MyUser.user.IsCercatore)
                            {
                                (BindingContext as MainPageViewModel).TendinaTextDetail = TEXTDETAIL_CATTURA_SEEKER;
                            }
                            else (BindingContext as MainPageViewModel).TendinaTextDetail = TEXTDETAIL_CATTURA_HIDER;

                            vibrationLength = TimeSpan.FromSeconds(2);
                            Vibration.Default.Vibrate(vibrationLength);
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

                //cambio colore
                InfoPartBorder.Stroke = originalStroke;
                //reset scritta
                (BindingContext as MainPageViewModel).TendinaTextDetail = "";

            } catch(TaskCanceledException ex) 
              { //BELLO QUI PALESE ESPLODE TUTTO RIP
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

