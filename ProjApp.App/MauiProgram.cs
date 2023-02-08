using Microsoft.Maui.LifecycleEvents;
using Plugin.Firebase.Android;
using Plugin.Firebase.Auth;
using Plugin.Firebase.Shared;
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
            .RegisterFirebaseServices()
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
        builder.Services.AddTransient<MainPageViewModel>();
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddSingleton<ProfilePageViewModel>();
        builder.Services.AddSingleton<ProfilePage>();
        builder.Services.AddTransient<LoginPageViewModel>();
        builder.Services.AddTransient<LoginPage>();
        

        return builder.Build();
	}

    private static MauiAppBuilder RegisterFirebaseServices(this MauiAppBuilder builder)
    {
        builder.ConfigureLifecycleEvents(events => {
#if IOS
            events.AddiOS(iOS => iOS.FinishedLaunching((app, launchOptions) => {
                CrossFirebase.Initialize(app, launchOptions, CreateCrossFirebaseSettings());
                return false;
            }));
#else
            events.AddAndroid(android => android.OnCreate((activity, state) =>
                CrossFirebase.Initialize(activity, state, CreateCrossFirebaseSettings())));
#endif
        });

        builder.Services.AddSingleton(_ => CrossFirebaseAuth.Current);
        return builder;
    }

    private static CrossFirebaseSettings CreateCrossFirebaseSettings()
    {
        return new CrossFirebaseSettings(
            isAuthEnabled: true,
            googleRequestIdToken: "466882726164-rsv7n5beus2dovt9q2gqemlc0d0f0831.apps.googleusercontent.com");
    }
}

