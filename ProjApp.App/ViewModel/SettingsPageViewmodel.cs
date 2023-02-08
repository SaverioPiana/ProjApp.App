using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjApp.ViewModel
{
    public partial class SettingsPageViewModel : ObservableObject
    {

        public SettingsPageViewModel() { }

        [RelayCommand]
        Task NavigateToLoginPage() => Shell.Current.GoToAsync($"{nameof(LoginPage)}", false);

        [RelayCommand]
        Task NavigateToProfilePage() => Shell.Current.GoToAsync("..",true);

    }
}
