using CommunityToolkit.Mvvm.Messaging;
using ProjApp.ViewModel;
#if ANDROID
using static ProjApp.MainActivity;
#endif

namespace ProjApp;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginPageViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        #if ANDROID
        WeakReferenceMessenger.Default.Send(new FullScreenMessage("HideOsNavigationBar")); 
        #endif
        (BindingContext as LoginPageViewModel).Constructor();
    }
}