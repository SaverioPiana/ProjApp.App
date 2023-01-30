using ProjApp.ViewModel;

namespace ProjApp;

public partial class LoginPage : ContentPage
{
    //public LoginPage() {}

    public LoginPage(LoginPageViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }



}