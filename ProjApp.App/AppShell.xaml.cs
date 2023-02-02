

namespace ProjApp;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

        Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));

        Routing.RegisterRoute(nameof(StartPage), typeof(StartPage));

        Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));

        Routing.RegisterRoute(nameof(ProfilePage), typeof(ProfilePage));

        Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
    }
}
