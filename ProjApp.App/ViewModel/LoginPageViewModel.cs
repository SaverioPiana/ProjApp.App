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



        [RelayCommand]
        Task NavigateToStartPage() => Shell.Current.GoToAsync(nameof(StartPage));



    }
}
