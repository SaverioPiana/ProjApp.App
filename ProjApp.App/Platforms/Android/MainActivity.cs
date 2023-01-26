using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace ProjApp;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    public class FullScreenMessage : ValueChangedMessage<object>
    {
        public FullScreenMessage(object r) : base(r)
        {
        }
    }

    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        WeakReferenceMessenger.Default.Register<FullScreenMessage>(this, (r, m) =>
        {
            IWindowInsetsController wicController = Window.InsetsController;
            Window.SetDecorFitsSystemWindows(false);
            Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);

            if (wicController != null)
            {
                wicController.Hide(WindowInsets.Type.NavigationBars());
            }
        });
    }
}
