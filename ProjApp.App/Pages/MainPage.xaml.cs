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
using static ProjApp.Gioco.GameLogic;
#if ANDROID
using static ProjApp.MainActivity;
#endif

namespace ProjApp;

public partial class MainPage : ContentPage
{
    public static CancellationTokenSource _cancelTokenSourceAvviso = null;

    Brush originalStroke;

    private bool TendinaAvvisoBehavior { get; set; } = false;

    public MainPage(MainPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private async void AvvisoRicevuto()
    {
        originalStroke = InfoPartBorder.Stroke;
        WeakReferenceMessenger.Default.Register<OpenAvvisoMessage>(this, async (r, m) =>
        {
            TendinaAvvisoBehavior = true;
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
                        case (AVVISO_INIZIO):
                            if (MyUser.user.IsCercatore)
                            {
                                (BindingContext as MainPageViewModel).TendinaTextDetail = TEXTDETAIL_INIZIO_SEEKER;
                            }
                            else (BindingContext as MainPageViewModel).TendinaTextDetail = TEXTDETAIL_INIZIO_HIDER;

                            vibrationLength = TimeSpan.FromSeconds(1);
                            Vibration.Default.Vibrate(vibrationLength);
                            break;

                        case (AVVISO_RUOLO):
                            if (MyUser.user.IsCercatore)
                            {
                                (BindingContext as MainPageViewModel).TendinaTextDetail = TEXTDETAIL_RUOLO_SEEKER;
                            }
                            else (BindingContext as MainPageViewModel).TendinaTextDetail = TEXTDETAIL_RUOLO_HIDER;

                            vibrationLength = TimeSpan.FromSeconds(1);
                            Vibration.Default.Vibrate(vibrationLength);
                            break;

                        case (AVVISO_TANATO):
                            if (MyUser.user.IsCercatore)
                            {
                                (BindingContext as MainPageViewModel).TendinaTextDetail = TEXTDETAIL_TANATO_SEEKER;
                            }
                            else (BindingContext as MainPageViewModel).TendinaTextDetail = TEXTDETAIL_TANATO_HIDER;

                            vibrationLength = TimeSpan.FromSeconds(1.5);
                            Vibration.Default.Vibrate(vibrationLength);
                            break;

                        case (AVVISO_NOTIFICA):
                            if (MyUser.user.IsCercatore)
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

                        case (AVVISO_MATCH_OVER):
                            TendinaAvvisoBehavior = false;
                            string messaggioFinePartita = TEXTDETAIL_MATCHOVER;
                            if (MyUser.user.IsCercatore)
                            {
                                List<string> presi = MyUser.user.NicknameGiocatoriPresi;
                                messaggioFinePartita += presi.Count!=0 ? $"hai catturato {presi.Count} buddies!\n" +
                                "Tra questi ci sono:\n" : "non hai catturato nessuno, hai perso!\n";
                                //per ogni giocatore che abbiamo preso lo
                                foreach (var presoNick in presi)
                                {
                                    messaggioFinePartita += "- " + presoNick + "\n";
                                }
                            }
                            else
                            {
                                messaggioFinePartita += MyUser.user.IsPreso ? "sei stato preso quindi hai perso\n" :
                                                        (MyUser.user.IsSalvo ? "ti sei tanato quindi hai vinto!\n"
                                                        : "è finito il tempo\ne non sei riuscito a tanarti, hai perso!\n");
                            }

                            (BindingContext as MainPageViewModel).TendinaTextDetail = messaggioFinePartita + "Per uscire clicca su \"Abbandona Partita\"";

                            vibrationLength = TimeSpan.FromSeconds(2);
                            Vibration.Default.Vibrate(vibrationLength);
                            break;
                    }
                });

                

                //se la partita è finita facciamo che l'avviso rimane
                if (m.Value.EventType.Equals(AVVISO_MATCH_OVER))
                {
                    await OpenDrawer(openY, _cancelTokenSourceAvviso.Token);
                }
                else
                {
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
                }
            }
            catch (TaskCanceledException ex)
            { //BELLO QUI PALESE ESPLODE TUTTO RIP
            }
            TendinaAvvisoBehavior = false;
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

        //registro la pagina per i messaggi con il viewmodel
        //BAD PRACTICE -----> MA DOBBIAMO CONSEGNARE, NON C'E' TEMPO
        AvvisoRicevuto();

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
                BottomDrawer.TranslationY = TendinaAvvisoBehavior ? 
                    APERTURA_TENDINA_AVVISI + e.TotalY : openY + e.TotalY;
            }

        }
        else if (e.StatusType == GestureStatus.Completed)
        {
            double minimunForClose = 70;
            if (TendinaAvvisoBehavior)
            {
                minimunForClose = 30;
                TendinaAvvisoBehavior = false; 
            }

            //Debug.WriteLine($"Completed: {e.TotalY}");
            if (lastPanY < minimunForClose)
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

