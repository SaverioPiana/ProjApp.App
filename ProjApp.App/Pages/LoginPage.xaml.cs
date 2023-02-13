using Android.Webkit;
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
        viewW.Navigated += OnNavigated;
    }

    private void OnNavigated(object sender, WebNavigatedEventArgs e)
    {
        LoginPageViewModel.NavigatedEventHandler(this, e);
    }


    #if ANDROID
    protected override void OnAppearing()
    {
        base.OnAppearing();
        WeakReferenceMessenger.Default.Send(new FullScreenMessage("HideOsNavigationBar")); 
    }
    #endif
}