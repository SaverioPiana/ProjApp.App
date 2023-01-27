using CommunityToolkit.Mvvm.Messaging;
#if ANDROID
using static ProjApp.MainActivity;
#endif

namespace ProjApp.Pages;

public partial class Login : ContentPage
{
    //serve ad entrare in fullscreen
    #if ANDROID
    protected override void OnAppearing()
    {
        base.OnAppearing();
        WeakReferenceMessenger.Default.Send(new FullScreenMessage("HideOsNavigationBar"));
    }
    #endif  
    public Login()
	{
		InitializeComponent();
	}
}