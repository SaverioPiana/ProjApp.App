

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
        Task NavigateToProfile() => Shell.Current.GoToAsync(nameof(ProfilePage));



    }
}
