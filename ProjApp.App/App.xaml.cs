

using Microsoft.Maui.Handlers;

namespace ProjApp;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        MainPage = new LoginPage(new ViewModel.LoginPageViewModel());
    }
}
