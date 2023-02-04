using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
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
        public string username;
        [ObservableProperty] 
        public string password;

        private bool succesfullLogin = true; ////////per ora true semrpe



        [RelayCommand]
        Task NavigateToStartPage() {
            if (succesfullLogin)
            {
                if(Shell.Current == null) 
                {
                    Application.Current.MainPage = new AppShell();
                }
                Shell.Current.GoToAsync($"//{nameof(ProfilePage)}?username={Username}");
            }
            //mandiamo un avviso alla pagina profilo: "ora è il momento di buildare lo user!!!"
            //(username è stato passato)
            Task.Run(() =>
            {
                Thread.Sleep(300);
                WeakReferenceMessenger.Default.Send(new BuildUserMessage(Username));
            });

            return Task.CompletedTask;
        }
    };
}
