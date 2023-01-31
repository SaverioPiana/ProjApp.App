using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ProjApp.ViewModel
{
    public partial class LobbyPageViewModel : ObservableObject
    {

        public LobbyPageViewModel() { }


        [RelayCommand]
        Task Back() => Shell.Current.GoToAsync("..");
    }
}