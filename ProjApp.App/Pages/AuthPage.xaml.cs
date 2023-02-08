using ProjApp.ViewModel;

namespace ProjApp;

public partial class AuthPage : ContentPage
{
	public AuthPage(AuthPageViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}