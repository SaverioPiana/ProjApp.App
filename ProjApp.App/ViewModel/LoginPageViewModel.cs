using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using ExCSS;
using Firebase.Auth;
using Firebase.Auth.Providers;
using Firebase.Auth.Repository;
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

        private bool succesfullLogin = true; ////////per ora true semrpe
        private bool firstTime = true;

        // Configure...
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
                WebAuthenticatorResult authResult = await WebAuthenticator.Default.AuthenticateAsync(
                    new Uri(uris),
                    new Uri("napp://"));

                string accessToken = authResult?.AccessToken;

                return string.Empty;

                //return credential
            });

            //    try
            //    {

            //        var client = new FirebaseAuthClient(config);
            //        await client.SignInWithRedirectAsync(FirebaseProviderType.Google, async uri =>
            //        {
            //            CiccioPasticcio = uri;
            //            await Task.Delay(6000);
            //            string result = await Application.Current.MainPage.DisplayPromptAsync("uri redirect?",
            //            $"vai qui {uri} e incolla il link di redirect", "Conferma", "Annulla",
            //            "red link");
            //            return result;
            //        });
            //        UserCredential user = await client.CreateUserWithEmailAndPasswordAsync(Username, Password);


            //    }
            //    catch(Exception ex)
            //    {

            //    }
            //}


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
}

