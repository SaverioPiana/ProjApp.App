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

        [ObservableProperty] string testoecodicepartita;
        [ObservableProperty] bool isCodiceVisible = false;

        [RelayCommand]
        Task NavigateToMainPage() => Shell.Current.GoToAsync(nameof(MainPage));
        
        [RelayCommand]
        async Task JoinLobby()
        {
            //faccio inserire il codice all'utente
            string result = await Application.Current.MainPage.DisplayPromptAsync("Join Lobby", "Inserisci il codice della Lobby","Conferma","Annulla","ID");
            MyUser.currPartita.IfCheckThenJoin(result);
        }

        [RelayCommand]
        async Task CreateLobby()
        {
            //NON CE ERROR HANDLING QUI
            MyUser.currPartita.CreateLobby();
            string codice = MyUser.currPartita.Cod_partita;
            IsCodiceVisible = true;
            Testoecodicepartita = "Codice partita: " + codice;
            //notifica per l'utente
            await Application.Current.MainPage.DisplayAlert("Partita creata", 
                $"Copia il codice della partita e\ninvialo agli altri giocatori", "Capito");
        }

    }
}
