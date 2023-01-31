

using Microsoft.Maui.Handlers;

namespace ProjApp;

public partial class App : Application
{
    public App(LoginPage page)
    {
        InitializeComponent();

        MainPage = page;
    }
}
