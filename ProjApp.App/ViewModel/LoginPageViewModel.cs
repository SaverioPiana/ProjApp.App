using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Firebase.Auth;
using Firebase.Auth.Providers;
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
        private static bool FIRST_TIME_LOGGING = true;

        public LoginPageViewModel()
        {
            Constructor();
        }

        public void Constructor()
        {
            Username = string.Empty;
            Password = string.Empty;
            if(Fbclient?.User != null)
            {
                Fbclient.SignOut();
            }
        }

        [ObservableProperty] 
        private string username;
        [ObservableProperty] 
        private string password;

        //sign in configs
        [ObservableProperty]
        private bool webviewvisible = false;

        [ObservableProperty]
        private bool othersarevisible = true;

        [ObservableProperty]
        private string sourceurl = string.Empty;

        private string redirectedUrl = string.Empty;

        //firebase auth
        [ObservableProperty]
        private FirebaseAuthClient fbclient;
        //


        FirebaseAuthConfig config = new FirebaseAuthConfig
        {
            ApiKey = "AIzaSyByATKL15ARJgSIxRHibyF-j2E3UUTNrWE",
            AuthDomain = "nascondapp.firebaseapp.com",
            Providers = new FirebaseAuthProvider[]
           {
                // Add and configure individual providers
                new EmailProvider(),
                new GithubProvider().AddScopes("user:email")

           },
        };

        [RelayCommand]
        public async void RegisterUserWithMail()
        {

            Fbclient = new FirebaseAuthClient(config);

            await Fbclient.SignInWithEmailAndPasswordAsync(Username, Password);
        }


        [RelayCommand]
        public async void RegisterUser()
        {
            if(FIRST_TIME_LOGGING)
            {
                Fbclient = new FirebaseAuthClient(config);
            }
            
            await Fbclient.SignInWithRedirectAsync(FirebaseProviderType.Github, async uris =>
            {
                Sourceurl = uris;
                Othersarevisible= false;
                Webviewvisible = true;
                while (!redirectedUrl.Contains(config.RedirectUri))
                {
                    await Task.Delay(200);
                }
                return redirectedUrl;
            });
            Fbclient.User?.ToString();
            if (Fbclient.User != null) await NavigateToStartPage();
        }

        public void OnNavigated(object sender, WebNavigatedEventArgs e)
        {
            if (e.Url.Contains(config.RedirectUri))
            { 
                Webviewvisible = false;
                Othersarevisible = true;
                redirectedUrl = e.Url;
            }
        }

        private async Task NavigateToStartPage() {
           
            
                //if (AppShell.Current == null)
                //{
                //    Application.Current.MainPage = new AppShell();
                //}
                await AppShell.Current.GoToAsync($"//{nameof(ProfilePage)}",false);


                //SOLO LA PRIMA VOLTA (da login -> profile)
                //passiamo lo username alla pagina profilo solo se ha gia interagito con lo user per il nickmname
                if (FIRST_TIME_LOGGING)
                {
                    WeakReferenceMessenger.Default.Register<ReadyToBuildUserMessage>(this, (r, m) =>
                    {
                        WeakReferenceMessenger.Default.Send(new BuildUserMessage(Fbclient.User.Uid));
                    });
                    FIRST_TIME_LOGGING = false;
                }
                else //la prifle page è gia stata creata ed è pronta a ricevere il nuovo username senza aspettare
                {
                    WeakReferenceMessenger.Default.Send(new BuildUserMessage(Fbclient.User.Uid));
                }

            }
        
    };

    internal class ReadyToBuildUserMessage : ValueChangedMessage<string>
    {
        public ReadyToBuildUserMessage(string value) : base(value)
        {
        }
    }
}
