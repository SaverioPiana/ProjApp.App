using CommunityToolkit.Mvvm.Messaging;
using ProjApp.ViewModel;
using static ProjApp.MainActivity;

namespace ProjApp;

public partial class MainPage : ContentPage 
{
	 

	public MainPage(MainPageViewModel viewModel)
	{
		InitializeComponent();

		BindingContext= viewModel;

		MainThread.BeginInvokeOnMainThread(LoginPageViewModel.SetNick);
	}

#if ANDROID
    protected override void OnAppearing()
    {
        base.OnAppearing();
        WeakReferenceMessenger.Default.Send(new FullScreenMessage("HideOsNavigationBar"));
    }
#endif

}