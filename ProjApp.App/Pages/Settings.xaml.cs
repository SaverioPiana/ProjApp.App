using ProjApp.ViewModel;

namespace ProjApp;

public partial class Settings : ContentPage{

    public Settings() { }

	public Settings(SettingsPageViewModel viewModel)
	{
        InitializeComponent();
        BindingContext = viewModel;
    }
}