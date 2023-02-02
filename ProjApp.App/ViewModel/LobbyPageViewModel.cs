using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ProjApp.ViewModel
{
    [QueryProperty(nameof(Codice), "codice")]
    public partial class LobbyPageViewModel : ObservableObject
    {

        public LobbyPageViewModel() { }


        [ObservableProperty] 
        private string codice;

        [RelayCommand]
        Task Back() => Shell.Current.GoToAsync("..");
    }
}