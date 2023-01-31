using CommunityToolkit.Mvvm.Messaging;
using ProjApp.ViewModel;


namespace ProjApp;

public partial class Settings : ContentPage{

    

	public Settings(SettingsPageViewModel viewModel)
	{
        InitializeComponent();
        BindingContext = viewModel;
    }

    #if ANDROID
    protected override void OnAppearing()
    {
        base.OnAppearing();
        WeakReferenceMessenger.Default.Send(new MainActivity.FullScreenMessage("HideOsNavigationBar"));
    }
    #endif
}