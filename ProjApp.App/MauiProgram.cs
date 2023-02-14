using ProjApp.ViewModel;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace ProjApp;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
            .UseSkiaSharp(true)
            .ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");

                fonts.AddFont("NewRodin_Pro_EB.otf", "NewRodin");
                fonts.AddFont("SuperMarioBrosWii.otf", "Mario");
                fonts.AddFont("Starjedi.ttf", "Jedi");
                fonts.AddFont("Glitch.otf", "Glitch");
            });

		builder.Services.AddSingleton<StartPageViewModel>();
        builder.Services.AddSingleton<StartPage>();
        builder.Services.AddSingleton<ProfilePageViewModel>();
        builder.Services.AddSingleton<ProfilePage>();
        builder.Services.AddSingleton<AppShell>();

        builder.Services.AddTransient<MainPageViewModel>();
        builder.Services.AddTransient<MainPage>();

        builder.Services.AddSingleton<LoginPageViewModel>();
        builder.Services.AddSingleton<LoginPage>();
        
        return builder.Build();
	}
}
