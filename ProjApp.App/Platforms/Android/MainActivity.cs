using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Microsoft.Identity.Client;
using MsalAuthInMaui.MsalClient;

namespace ProjApp;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    //private const string AndroidRedirectURI = $"msauth://com.companyname.projapp/snaHlgr4autPsfVDSBVaLpQXnqU=";
    private const string AndroidRedirectURI = $"msauth://com.companyname.nascondapp/B64377998E6E41DAA82BEE84EC2C7740";
    public class FullScreenMessage : ValueChangedMessage<object>
    {
        public FullScreenMessage(object r) : base(r)
        {
        }
    }

    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        // Configure platform specific parameters
        PlatformConfig.Instance.RedirectUri = AndroidRedirectURI;
        PlatformConfig.Instance.ParentWindow = this;

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

    /// <summary>
    /// This is a callback to continue with the authentication
    /// Info about redirect URI: https://docs.microsoft.com/en-us/azure/active-directory/develop/msal-client-application-configuration#redirect-uri
    /// </summary>
    /// <param name="requestCode">request code </param>
    /// <param name="resultCode">result code</param>
    /// <param name="data">intent of the actvity</param>
    protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
    {
        base.OnActivityResult(requestCode, resultCode, data);
        AuthenticationContinuationHelper.SetAuthenticationContinuationEventArgs(requestCode, resultCode, data);
    }
}
