using ProjApp.ViewModel;

namespace ProjApp;

public partial class ProfilePage : ContentPage
{
	public ProfilePage(ProfilePageViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
}