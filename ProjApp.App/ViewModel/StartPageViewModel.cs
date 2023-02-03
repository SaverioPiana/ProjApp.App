using ProjApp.MapEl.GPS;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace ProjApp.ViewModel
{
    public partial class StartPageViewModel : ObservableObject
    {

        public StartPageViewModel() { }

        [ObservableProperty]
        string codice;
        [ObservableProperty]
        bool isCodiceVisible = false;
        [ObservableProperty]
        bool isAdmin = false;
        [ObservableProperty]
        string nick = MyUser.Nick;

        [RelayCommand]
        Task AvviaPartita() => Shell.Current.GoToAsync(nameof(MainPage));

        [RelayCommand]
        Task CopyCode() 
        {
            Clipboard.Default.SetTextAsync(Codice);
            return Task.CompletedTask;
        }
        
        [RelayCommand]
        public void JoinLobby(Entry entry)
        {
            //faccio inserire il codice all'utente
            MyUser.currPartita.IfCheckThenJoin(entry.Text);
            if(IsAdmin) IsAdmin = false;
        }

        [RelayCommand]
        public void CreateLobby()
        {
            //NON CE ERROR HANDLING QUI
            MyUser.currPartita.CreateLobby();
            Codice = MyUser.currPartita.Cod_partita;
            IsCodiceVisible = true;
            IsAdmin = true;
        }
    }
}
