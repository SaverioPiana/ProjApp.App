

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;

namespace ProjApp.ViewModel
{
    public partial class StartPageViewModel : ObservableObject
    {

        public StartPageViewModel(){}

        [RelayCommand]
        Task NavigateToMainPage() => Shell.Current.GoToAsync(nameof(MainPage));

        [RelayCommand]
        Task NavigateToProfile() => Shell.Current.GoToAsync(nameof(ProfilePage));

        [RelayCommand]
        Task NavigateToSettings() => Shell.Current.GoToAsync(nameof(Settings));

        [RelayCommand]
        async Task JoinLobby()
        {
            string result = await Application.Current.MainPage.DisplayPromptAsync("Join Lobby", "Lobby id?");

        }

    }
}
