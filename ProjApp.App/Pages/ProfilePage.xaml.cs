using AndroidX.Lifecycle;
using CommunityToolkit.Mvvm.Messaging;
using ProjApp.ViewModel;
using static ProjApp.MainActivity;

namespace ProjApp;

public partial class ProfilePage : ContentPage
{
	public ProfilePage(ProfilePageViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }

    #if ANDROID
    protected override void OnAppearing()
    {
        base.OnAppearing();
        MainThread.BeginInvokeOnMainThread((BindingContext as ProfilePageViewModel).SetNick);
        (BindingContext as ProfilePageViewModel).BuildUser();
        WeakReferenceMessenger.Default.Send(new FullScreenMessage("HideOsNavigationBar"));
    }
    #endif
}