using CommunityToolkit.Mvvm.Messaging;
using ProjApp.ViewModel;
#if ANDROID
using static ProjApp.MainActivity;
#endif

namespace ProjApp;

public partial class StartPage : ContentPage 
{
	public StartPage(StartPageViewModel viewModel)
	{
		InitializeComponent();
		BindingContext= viewModel;
	}

#if ANDROID
    protected override void OnAppearing()
    {
        base.OnAppearing();
        WeakReferenceMessenger.Default.Send(new FullScreenMessage("HideOsNavigationBar"));
    }
#endif

}