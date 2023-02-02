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
                MainThread.BeginInvokeOnMainThread(SetNick);
                
                Application.Current.MainPage = new AppShell();
            }
            return Task.CompletedTask;
        }

        public async void SetNick()
        {
            if (MainThread.IsMainThread)
            {
                string retrievedNick = MyUser.RetrieveNickFromFile(MyUser.NICK_FILENAME);
                if (retrievedNick.Equals(""))
                {
                    string newnick;

                    newnick = await Application.Current.MainPage.DisplayPromptAsync("Come ti chiami?",
                    "Inserisci il nome che gli altri utenti visualizzeranno", "Conferma", "Annulla",
                    "Nickname");

                    MyUser.SaveLastNickOnFile(newnick);
                    MyUser.nick = newnick;
                }
                else
                {
                    MyUser.nick = retrievedNick;
                }
            }
            else Console.WriteLine("///////////////////NON STAI CHIAMANDO QUESTA SETNICK() DAL MAIN THREAD!!!!");
            
            MyUser.BuildMyUser(Username); //username sarebbe l'ID
        }

    };
}
