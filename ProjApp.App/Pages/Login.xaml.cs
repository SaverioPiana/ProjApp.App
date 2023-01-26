using CommunityToolkit.Mvvm.Messaging;
using static ProjApp.MainActivity;

namespace ProjApp.Pages;

public partial class Login : ContentPage
{
    //serve ad entrare in fullscreen
    protected override void OnAppearing()
    {
        base.OnAppearing();
        WeakReferenceMessenger.Default.Send(new FullScreenMessage("HideOsNavigationBar"));
    }
    public Login()
	{
		InitializeComponent();
	}
}