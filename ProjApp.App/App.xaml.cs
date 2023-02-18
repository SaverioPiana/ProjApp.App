

using Microsoft.Maui.Handlers;
using ProjApp.Gioco;

namespace ProjApp;

public partial class App : Application
{
    public App(AppShell shell)
    {
        InitializeComponent();

        //Signalr
        Connessione _ = new();
        Task.Run(() =>
        {
            Dispatcher.Dispatch(async () =>
            await Connessione.con.StartAsync());
        });

        MainPage = shell;
    }
}
