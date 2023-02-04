using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using ProjApp.MapEl.GPS;
using System.ComponentModel;

namespace ProjApp.ViewModel
{
    public partial class ProfilePageViewModel : ObservableObject
    {
        public ProfilePageViewModel() 
        {
            MainThread.BeginInvokeOnMainThread(SetNick);
            BuildUser();
        }

        [ObservableProperty]
        private string username;

        [ObservableProperty]
        public string nick;

        private bool firstTime = true;

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


        public class BuildUserMessage : ValueChangedMessage<string>
        {
            public BuildUserMessage(string value) : base(value)
            {
            }
        }

        public void BuildUser()
        {
            WeakReferenceMessenger.Default.Register<BuildUserMessage>(this, (r, m) => 
            {
                Username = m.Value;
                if (firstTime)
                {
                    MyUser.BuildMyUser(Username, Nick);
                    firstTime = false;
                }
                else MyUser.ChangeID(Username);
            });
        }
    }
}