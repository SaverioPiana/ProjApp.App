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
                fonts.AddFont("comici.ttf", "ComicSansRegular");
            });

        builder.Services.AddSingleton<SettingsPageViewModel>();
        builder.Services.AddSingleton<SettingsPage>();
		builder.Services.AddSingleton<StartPageViewModel>();
        builder.Services.AddSingleton<StartPage>();
        builder.Services.AddSingleton<ProfilePageViewModel>();
        builder.Services.AddSingleton<ProfilePage>();
        builder.Services.AddSingleton<AppShell>();

        builder.Services.AddSingleton<MainPageViewModel>();
        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddSingleton<LoginPageViewModel>();
        builder.Services.AddSingleton<LoginPage>();
        
        return builder.Build();
	}
}
