using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProjApp.MapEl.GPS;
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

        [ObservableProperty] public string username;
        [ObservableProperty] public string password;
        private bool succesfullLogin = true; ////////per ora true semrpe



        [RelayCommand]
        Task NavigateToStartPage() {
            if (succesfullLogin)
            {
                MyUser.BuildMyUser(Username); //username sarebbe l'ID/mail
                Application.Current.MainPage = new AppShell();
            }
            return Task.CompletedTask;
        }

        

    };
}
