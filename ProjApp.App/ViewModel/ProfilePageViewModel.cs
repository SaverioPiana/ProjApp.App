using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProjApp.MapEl.GPS;
using System.ComponentModel;

namespace ProjApp.ViewModel
{
    [QueryProperty(nameof(Username), "username")]
    public partial class ProfilePageViewModel : ObservableObject
    {
        public ProfilePageViewModel() { }

        [ObservableProperty]
        private string username;

        [ObservableProperty]
        public string nick;

        [RelayCommand]
        Task NavigateToSettingsPage() => Shell.Current.GoToAsync(nameof(SettingsPage));

        public async void SetNick()
        {
            if (MainThread.IsMainThread)
            {
                string retrievedNick = MyUser.RetrieveNickFromFile(MyUser.NICK_FILENAME);
                if (retrievedNick.Equals("") || retrievedNick == null)
                {
                    string newnick;

                    newnick = await Application.Current.MainPage.DisplayPromptAsync("Come ti chiami?",
                    "Inserisci il nome che gli altri utenti visualizzeranno", "Conferma", "Annulla",
                    "Nickname");

                    MyUser.SaveLastNickOnFile(newnick);
                    Nick = newnick;
                }
                else Nick = retrievedNick; //funzionera INotifyPropertyChanged??? ----> SI
            }
            else Console.WriteLine("///////////////////NON STAI CHIAMANDO QUESTA SETNICK() DAL MAIN THREAD!!!!");
        }

        public void BuildUser()
        {
            MyUser.BuildMyUser(Username, Nick);
        }
    }
}