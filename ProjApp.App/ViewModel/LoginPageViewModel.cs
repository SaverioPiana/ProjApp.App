using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using ExCSS;
using Firebase.Auth;
using Firebase.Auth.Providers;
using Firebase.Auth.Repository;
using Javax.Security.Auth;
using ProjApp.MapEl.GPS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ProjApp.ViewModel.ProfilePageViewModel;

namespace ProjApp.ViewModel
{
    public partial class LoginPageViewModel : ObservableObject
    {
        public LoginPageViewModel() { }

        [ObservableProperty]
        private string username;

        [ObservableProperty]
        private string password;

        [ObservableProperty]
        private string ciccioPasticcio;

        [ObservableProperty]
        private string sourceurl = string.Empty;

        [ObservableProperty]
        private bool webviewvisible = false;
        private bool succesfullLogin = true; ////////per ora true semrpe
        private bool firstTime = true;
        // Configure...

        private static string redirectedUrl = String.Empty;


        FirebaseAuthConfig config = new FirebaseAuthConfig
        {
            ApiKey = "AIzaSyByATKL15ARJgSIxRHibyF-j2E3UUTNrWE",
            AuthDomain = "nascondapp.firebaseapp.com",
            Providers = new FirebaseAuthProvider[]
            {
                // Add and configure individual providers
                new GoogleProvider().AddScopes("email"),
                new EmailProvider(),
                new GithubProvider().AddScopes("user:email")

            },

        };

        [RelayCommand]
        public async void RegisterUser()
        {

            var client = new FirebaseAuthClient(config);

            var userCredential = await client.SignInWithRedirectAsync(FirebaseProviderType.Github, async uris =>
            {
                Sourceurl = uris;
                Webviewvisible = true;
                while (!redirectedUrl.Contains(config.RedirectUri))
                {
                    await Task.Delay(100);
                }
                Webviewvisible = false;
                return redirectedUrl;



            });
            userCredential.AuthCredential.ToString();

        }

        public static void NavigatedEventHandler( object sender, WebNavigatedEventArgs e)
        {
            redirectedUrl = e.Url;
            
        }

        [RelayCommand]
        Task NavigateToStartPage()
        {
            if (succesfullLogin)
            {
                if (Shell.Current == null)
                {
                    Application.Current.MainPage = new AppShell();
                }
                Shell.Current.GoToAsync($"//{nameof(ProfilePage)}");
            }

            //SOLO LA PRIMA VOLTA (da login -> profile)
            //passiamo lo username alla pagina profilo solo se ha gia interagito con lo user per il nickmname
            if (firstTime)
            {
                WeakReferenceMessenger.Default.Register<ReadyToBuildUserMessage>(this, (r, m) =>
                {
                    WeakReferenceMessenger.Default.Send(new BuildUserMessage(Username));
                });
            }
            else //la prifle page è gia stata creata ed è pronta a ricevere il nuovo username senza aspettare
            {
                WeakReferenceMessenger.Default.Send(new BuildUserMessage(Username));
            }

            return Task.CompletedTask;
        }
    }

    internal class ReadyToBuildUserMessage : ValueChangedMessage<string>
    {
        public ReadyToBuildUserMessage(string value) : base(value)
        {
        }
    }
}



