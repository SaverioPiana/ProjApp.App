using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProjApp.MapEl.GPS;
using System.ComponentModel;

namespace ProjApp.ViewModel
{
    
    public partial class ProfilePageViewModel : ObservableObject
    {


        public ProfilePageViewModel() { }

        [ObservableProperty]
        public string nick;

        [RelayCommand]
        Task NavigateToSettingsPage() => Shell.Current.GoToAsync(nameof(SettingsPage));

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
                    MyUser.Nick = newnick;
                }
                else
                {
                    MyUser.Nick = retrievedNick;
                }
                Nick = MyUser.Nick; //funzionera INotifyPropertyChanged??? ----> SI
            }
            else Console.WriteLine("///////////////////NON STAI CHIAMANDO QUESTA SETNICK() DAL MAIN THREAD!!!!");
        }

    }
}