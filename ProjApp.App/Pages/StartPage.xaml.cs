using ProjApp.ViewModel;


namespace ProjApp;

public partial class StartPage : ContentPage 
{
	 

	public StartPage(StartPageViewModel viewModel)
	{
		InitializeComponent();

		BindingContext= viewModel;
		
	}
}