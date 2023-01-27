using CommunityToolkit.Mvvm.Messaging;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;
using ProjApp.Gioco;
using ProjApp.MapEl.GPS;
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
        //spostato qua da fixare
        MyUser.BuildMyUser();
	}


    private async void google_auth()
    {
        
    }


}