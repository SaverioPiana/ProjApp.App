using Microsoft.Maui.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

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
            string result = await Application.Current.MainPage.DisplayPromptAsync("Join Lobby", "Lobby id?");

        }
    }
}
