using ProjApp.ViewModel;

namespace ProjApp;

public partial class EndPage : ContentPage
{
	public EndPage(EndPageViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}