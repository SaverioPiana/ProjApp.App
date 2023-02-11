using Android.App;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Maui.Animations;
using Microsoft.Maui.Controls;
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

    //PIU O MENO FUNZIONA, MI MANCA UN PEZZO    

    //private async void OnClickedCreaPartitaOrJoin(object sender, EventArgs e)
    //{
    //    Row0.Height = 50;
    //    Row1.Height = 50;
    //    Row2.Height = 50;
    //    Row3.Height = 50;
    //    Row4.Height = 50;
    //    await LobbyBorder.TranslateTo(TranslationX, -230, 500, Easing.CubicInOut);
    //    LobbyBorderRow.Height = GridLength.Star;
    //    //Row0.Height = GridLength.Auto;
    //    //Row1.Height = GridLength.Auto;
    //    //Row2.Height = GridLength.Auto;
    //    //Row3.Height = GridLength.Auto;
    //    //Row4.Height = GridLength.Auto;
    //}

}