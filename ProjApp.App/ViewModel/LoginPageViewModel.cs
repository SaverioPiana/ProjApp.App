using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjApp.ViewModel
{
    public partial class LoginPageViewModel : ObservableObject
    {

        public LoginPageViewModel() { }

        [ObservableProperty] private string _username;
        [ObservableProperty] public string _password;
        private bool succesfullLogin = true; ////////per ora true semrpe

        

        [RelayCommand]
        Task NavigateToStartPage() {
            if(succesfullLogin) {
                Application.Current.MainPage = new AppShell();
                Shell.Current.GoToAsync(nameof(ProfilePage));
            }
            return Task.CompletedTask;
        }

    };
}
