using CommunityToolkit.Mvvm.Input;

namespace ProjApp.ViewModel
{
    public partial class EndPageViewModel
    {

        [RelayCommand]
        private async Task ReturnToStartPage()
        {
            await AppShell.Current.GoToAsync("../..", false);
        }

    }
}
