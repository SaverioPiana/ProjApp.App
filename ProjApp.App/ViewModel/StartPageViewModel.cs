using ProjApp.MapEl.GPS;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Globalization;

namespace ProjApp.ViewModel
{
    public partial class StartPageViewModel : ObservableObject
    { 

        public StartPageViewModel(){}

        [ObservableProperty] 
        string codice;
        [ObservableProperty]
        bool isCodiceVisible = false;
        [ObservableProperty] 
        bool isAdmin = false;
        [ObservableProperty]
        string nick = MyUser.Nick;

        [RelayCommand]
        Task NavigateToMainPage() => Shell.Current.GoToAsync(nameof(MainPage));
        
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
        }
    }
}
