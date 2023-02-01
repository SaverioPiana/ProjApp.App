using CommunityToolkit.Mvvm.Messaging;
using ProjApp.ViewModel;
using static ProjApp.MainActivity;

namespace ProjApp;

public partial class SettingsPage : ContentPage{

    public SettingsPage() { }

	public SettingsPage(SettingsPageViewModel viewModel)
	{
        InitializeComponent();
        BindingContext = viewModel;
    }

    #if ANDROID
    protected override void OnAppearing()
    {
        base.OnAppearing();
        WeakReferenceMessenger.Default.Send(new FullScreenMessage("HideOsNavigationBar"));
    }
    #endif
}