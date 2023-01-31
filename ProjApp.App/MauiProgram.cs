using ProjApp.ViewModel;
using SkiaSharp.Views.Maui.Controls.Hosting;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace ProjApp;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
            .UseSkiaSharp()
            .ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("comici.ttf", "ComicSansRegular");
            });

        var executingAssembly = Assembly.GetExecutingAssembly();

        using var stream = executingAssembly.GetManifestResourceStream("ProjApp.appsettings.json");

        var configuration  = new ConfigurationBuilder()
            .AddJsonStream(stream)
            .Build();

		builder.Services.AddSingleton<StartPageViewModel>();
        builder.Services.AddSingleton<StartPage>();
        builder.Services.AddSingleton<MainViewModel>();
        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddSingleton<ProfilePageViewModel>();
        builder.Services.AddSingleton<ProfilePage>();
        builder.Services.AddTransient<LoginPageViewModel>();
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddSingleton<SettingsPageViewModel>();
        builder.Services.AddSingleton<Settings>();


        builder.Configuration
            .AddConfiguration(configuration);

        return builder.Build();
	}
}
