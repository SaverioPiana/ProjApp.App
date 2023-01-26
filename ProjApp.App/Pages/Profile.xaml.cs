using CommunityToolkit.Mvvm.Messaging;
using static ProjApp.MainActivity;

namespace ProjApp.Pages;

public partial class Profile : ContentPage { 

    //serve ad entrare in fullscreen
    protected override void OnAppearing()
    {
        base.OnAppearing();
        WeakReferenceMessenger.Default.Send(new FullScreenMessage("HideOsNavigationBar"));
    }
    public Profile()
	{

		InitializeComponent();
	}
}