using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using ProjApp.MsalClient;

namespace ProjApp.ViewModel
{
    public partial class LoginPageViewModel : ObservableObject
    {
        private string _accessToken = string.Empty;
        private PCAWrapper _pcaWrapper;
        private PCASocialWrapper _pcaSocialWrapper;
        private IConfiguration _configuration;

        bool _isLoggedIn = false;
        public bool IsLoggedIn
        {
            get => _isLoggedIn;
            set
            {
                if (value == _isLoggedIn) return;
                _isLoggedIn = value;
                OnPropertyChanged(nameof(IsLoggedIn));
            }
        }

        public LoginPageViewModel(IConfiguration configuration)
        {
            _configuration = configuration;
            _pcaWrapper = new PCAWrapper(_configuration);
            _pcaSocialWrapper = new PCASocialWrapper(_configuration);

            _ = Login(_pcaWrapper);
        }

        [ObservableProperty] private string _username;
        [ObservableProperty] public string _password;
        private bool succesfullLogin = true; ////////per ora true semrpe



        [RelayCommand]
        Task NavigateToStartPage()
        {
            if (succesfullLogin)
            {
                Application.Current.MainPage = new AppShell();
                Shell.Current.GoToAsync(nameof(ProfilePage));
            }
            return Task.CompletedTask;
        }
        [RelayCommand]
        async void LoginButtonClicked()
        {
            await Login(_pcaWrapper).ConfigureAwait(false);

        }
        [RelayCommand]
        async void LoginWithTwitterButtonClicked()
        {
            await Login(_pcaSocialWrapper).ConfigureAwait(false);
        }

        private async Task Login(IPCAWrapper pcaWrapper)
        {
            try
            {
                // Attempt silent login, and obtain access token.
                var result = await pcaWrapper.AcquireTokenSilentAsync(pcaWrapper.Scopes).ConfigureAwait(false);
                IsLoggedIn = true;

                // Set access token.
                _accessToken = result.AccessToken;

                // Display Access Token from AcquireTokenSilentAsync call.
                await ShowOkMessage("Access Token from AcquireTokenSilentAsync call", _accessToken).ConfigureAwait(false);
            }
            // A MsalUiRequiredException will be thrown, if this is the first attempt to login, or after logging out.
            catch (MsalUiRequiredException)
            {
                try
                {
                    // Perform interactive login, and obtain access token.
                    var result = await pcaWrapper.AcquireTokenInteractiveAsync(pcaWrapper.Scopes).ConfigureAwait(false);
                    IsLoggedIn = true;

                    // Set access token.
                    _accessToken = result.AccessToken;

                    // Display Access Token from AcquireTokenInteractiveAsync call.
                    await ShowOkMessage("Access Token from AcquireTokenInteractiveAsync call", _accessToken).ConfigureAwait(false);
                }
                catch(Exception ex)
                {
                    // Ignore.
                }
            }
            catch (Exception ex)
            {
                IsLoggedIn = false;
                await ShowOkMessage("Exception in AcquireTokenSilentAsync", ex.Message).ConfigureAwait(false);
            }
        }
        [RelayCommand]
        async void LogoutButtonClicked()
        {
            // Log out from Microsoft.
            await _pcaWrapper.SignOutAsync().ConfigureAwait(false);

            // Log out from Social.
            await _pcaSocialWrapper.SignOutAsync().ConfigureAwait(false);

            await ShowOkMessage("Signed Out", "Sign out complete.").ConfigureAwait(false);
            IsLoggedIn = false;
            _accessToken = string.Empty;


        }
        [RelayCommand]
        async private void GetWeatherForecastButtonClicked()
        {
            // Call the Secure Web API to get the weatherforecast data.
            var weatherForecastData = await CallSecureWebApi(_accessToken).ConfigureAwait(false);

            // Show the data.
            if (weatherForecastData != string.Empty)
                await ShowOkMessage("WeatherForecast data", weatherForecastData).ConfigureAwait(false);
        }

        private static async Task<string> CallSecureWebApi(string accessToken)
        {
            if (accessToken == string.Empty)
                return string.Empty;

            try
            {
                // Get the weather forecast data from the Secure Web API.
                var client = new HttpClient();

                // Create the request.
                var message = new HttpRequestMessage(HttpMethod.Get, "{https://nascondappmsalsecurewebapi.azurewebsites.net/weatherforecast");

                // Add the Authorization Bearer header.
                message.Headers.Add("Authorization", $"Bearer {accessToken}");

                // Send the request.
                var response = await client.SendAsync(message).ConfigureAwait(false);

                // Get the response.
                var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                // Ensure a success status code.
                response.EnsureSuccessStatusCode();

                // Return the response.
                return responseString;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }


        private Task ShowOkMessage(string title, string message)
        {
            
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Application.Current.MainPage.DisplayAlert(title, message, "OK");
            });
            return Task.CompletedTask;



        }
    }
}
