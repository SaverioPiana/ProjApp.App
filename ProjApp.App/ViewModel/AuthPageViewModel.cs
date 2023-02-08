using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Plugin.Firebase.Auth;
using ProjApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProjApp.ViewModel
{
    public partial class AuthPageViewModel : ObservableObject
    {
        private readonly IAuthService _authService;


        public AuthPageViewModel(IAuthService authService)
        {
            _authService = authService;
           

        }
        [RelayCommand]
        public async void GoogleAuth()
        {
            
            MainThread.BeginInvokeOnMainThread(async() => {try
            {
                if (_authService != null)
                {
                        Task.Run(async () =>
                        {
                            IFirebaseUser googleUser;
                            try  {googleUser = await _authService.LinkWithGoogle();
                            }
                            catch(Exception ex) {
                                googleUser = await _authService.LinkWithGoogle();
                            }

                        });
                    //if (googleUser != null)
                    //{
                    //    await Shell.Current.DisplayAlert(
                    //        "googleUser",
                    //        $"{googleUser.Email} , {googleUser.DisplayName} , {googleUser}",
                    //        "OK");
                    //}
                    //if (_authService.CurrentUser != null)
                    //{
                    //    await Shell.Current.DisplayAlert(
                    //        "firebaseAuth.CurrentUser",
                    //        $"{_authService.CurrentUser.Email} , {_authService.CurrentUser.DisplayName}",
                    //        "OK");
                    //}

                }
            }
            catch (Exception e)
            {
                await Shell.Current.DisplayAlert(
                   "Error",
                   $"{e.Message}",
                   "OK");
            } }); 
            
        }
    }
}
