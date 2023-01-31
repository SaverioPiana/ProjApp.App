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

        [ObservableProperty] private string _username;
        [ObservableProperty] public string _password;
        private bool succesfullLogin = true; ////////per ora true semrpe

        

        [RelayCommand]
        Task NavigateToStartPage() {
            if(succesfullLogin) {
                string retrievedNick = MyUser.RetrieveNickFromFile(MyUser.NICK_FILENAME);
                if (retrievedNick.Equals(""))
                {
                    string nick = Application.Current.MainPage.DisplayPromptAsync("Come ti chiami?",
                        "Inserisci il nome che gli altri utenti visualizzeranno", "Conferma", "Annulla", "Nickname", 15).Result;
                    MyUser.SaveLastNickOnFile(nick);
                    MyUser.nick = nick;
                }
                else
                {
                    MyUser.nick = retrievedNick;
                }

                MyUser.BuildMyUser(Username); //username sarebbe l'ID
                Application.Current.MainPage = new AppShell();
                Shell.Current.GoToAsync(nameof(ProfilePage));
            }
            return Task.CompletedTask;
        }

    };
}
