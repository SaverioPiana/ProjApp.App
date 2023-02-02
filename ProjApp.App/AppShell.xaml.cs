

namespace ProjApp;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

        Routing.RegisterRoute($"///{nameof(LoginPage)}", typeof(LoginPage));

        Routing.RegisterRoute($"///{nameof(MainPage)}", typeof(MainPage));

        Routing.RegisterRoute($"///{nameof(MainPage)}/{nameof(MatchPage)}", typeof(MatchPage));

        Routing.RegisterRoute($"///{nameof(ProfilePage)}", typeof(ProfilePage));

        Routing.RegisterRoute($"///{nameof(SettingsPage)}", typeof(SettingsPage));

        Routing.RegisterRoute($"///{nameof(MainPage)}/{nameof(LobbyPage)}", typeof(LobbyPage));
    }
}
