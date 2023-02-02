using ProjApp.MapEl.GPS;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ProjApp.ViewModel
{
    public partial class MainPageViewModel : ObservableObject
    { 

        public MainPageViewModel(){}

        [ObservableProperty] 
        string codice;
        [ObservableProperty] 
        bool isCodiceVisible = false;
        [ObservableProperty] 
        bool isAdmin = false;

        [RelayCommand]
        Task NavigateToMatchPage() => Shell.Current.GoToAsync(nameof(MatchPage));
        
        [RelayCommand]
        public async Task JoinLobby()
        {
            //faccio inserire il codice all'utente
            string result = await Application.Current.MainPage.DisplayPromptAsync("Join Lobby", "Inserisci il codice della Lobby","Conferma","Annulla","ID");
            MyUser.currPartita.IfCheckThenJoin(result);
            if(IsAdmin) IsAdmin = false;
        }

        [RelayCommand]
        public async Task CreateLobby()
        {
            //NON CE ERROR HANDLING QUI
            MyUser.currPartita.CreateLobby();
            Codice = MyUser.currPartita.Cod_partita;
            IsCodiceVisible = true;
            IsAdmin = true;
            //vado nella pagina Lobby
            await Shell.Current.GoToAsync($"///{nameof(MainPage)}/{nameof(LobbyPage)}?codice={Codice}");
        }
    }
}
