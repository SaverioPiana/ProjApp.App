using CommunityToolkit.Mvvm.Messaging;
using ProjApp.ViewModel;


namespace ProjApp;

public partial class SettingsPage : ContentPage{

    public SettingsPage() { }

	public SettingsPage(SettingsPageViewModel viewModel)
	{
        InitializeComponent();
        BindingContext = viewModel;
    }

    
    protected override void OnAppearing()
    {
        base.OnAppearing();
        WeakReferenceMessenger.Default.Send(new FullScreenMessage("HideOsNavigationBar"));
    }
   
}