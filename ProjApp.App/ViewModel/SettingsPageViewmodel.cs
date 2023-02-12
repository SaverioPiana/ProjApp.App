using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ProjApp.ViewModel
{
    public partial class SettingsPageViewModel : ObservableObject
    {

        public SettingsPageViewModel() { }

        [RelayCommand]
        private async Task NavigateToLoginPage() => await AppShell.Current.GoToAsync($"//{nameof(LoginPage)}", false);

        [RelayCommand]
        private async Task NavigateToProfilePage() => await AppShell.Current.GoToAsync("..",true);

    }
}
