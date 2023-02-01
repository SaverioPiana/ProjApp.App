using Microsoft.Maui.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProjApp.MapEl.GPS;
using Microsoft.AspNetCore.SignalR.Client;
using ProjApp.Gioco;

namespace ProjApp.ViewModel
{
    public partial class MainPageViewModel : ObservableObject
    {

        public MainPageViewModel(){}

        [ObservableProperty] string codice;
        [ObservableProperty] bool isCodiceVisible = false;
        [ObservableProperty] bool isAdmin = false;

        [RelayCommand]
        Task NavigateToMainPage() => Shell.Current.GoToAsync(nameof(MainPage));
        
        [RelayCommand]
        async Task JoinLobby()
        {
            //faccio inserire il codice all'utente
            string result = await Application.Current.MainPage.DisplayPromptAsync("Join Lobby", "Inserisci il codice della Lobby","Conferma","Annulla","ID");
            MyUser.currPartita.IfCheckThenJoin(result);
            if(IsAdmin) IsAdmin = false;
        }

        [RelayCommand]
        async Task CreateLobby()
        {
            //NON CE ERROR HANDLING QUI
            MyUser.currPartita.CreateLobby();
            Codice = MyUser.currPartita.Cod_partita;
            IsCodiceVisible = true;
            IsAdmin = true;
            //vado nella pagina Lobby
            await Shell.Current.GoToAsync($"../{nameof(LobbyPage)}");
        }
    }
}
