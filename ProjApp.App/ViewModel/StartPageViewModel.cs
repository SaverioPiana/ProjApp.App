using Microsoft.Maui.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProjApp.MapEl.GPS;
using Microsoft.AspNetCore.SignalR.Client;
using ProjApp.Gioco;

namespace ProjApp.ViewModel
{
    public partial class StartPageViewModel : ObservableObject
    {

        public StartPageViewModel(){}

        [RelayCommand]
        Task NavigateToMainPage() => Shell.Current.GoToAsync(nameof(MainPage));
        
        [RelayCommand]
        async Task JoinLobby()
        {
            string result = await Application.Current.MainPage.DisplayPromptAsync("Join Lobby", "Inserisci il codice della Lobby","Conferma","Annulla","ID");
            MyUser.currPartita.JoinLobby(result);
        }

        [RelayCommand]
        async Task CreateLobby()
        {
            MyUser.currPartita.CreateLobby();
            string codice = MyUser.currPartita.Cod_partita;
            await Application.Current.MainPage.DisplayAlert("Partita creata", 
                $"Questo e' il codice della partita,\ninvialo agli altri giocatori : {codice}", "Conferma");
        }

    }
}
