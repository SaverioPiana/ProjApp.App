using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using ProjApp.MapEl.GPS;
using System.ComponentModel;
using static ProjApp.ViewModel.StartPageViewModel;

namespace ProjApp.ViewModel
{
    public partial class ProfilePageViewModel : ObservableObject
    {
        public ProfilePageViewModel() 
        {
            BuildUser();
            MainThread.BeginInvokeOnMainThread(SetNick);
        }

        [ObservableProperty]
        private string username;
        [ObservableProperty]
        public string nick;
        [ObservableProperty]
        private bool changingNick = false;
        [ObservableProperty]
        private bool doneChangingNickname = true;

        private bool firstTime = true;

        [RelayCommand]
        private async Task NavigateToSettingsPage() => await AppShell.Current.GoToAsync(nameof(SettingsPage));

        [RelayCommand]
        public void ChangeNick()
        {
            ChangingNick = true;
            DoneChangingNickname = false;
        }

        [RelayCommand]
        public void DoneChangingNick()
        {
            MyUser.ChangeNick(Nick);
            WeakReferenceMessenger.Default.Send<UIChangeAlertStartPage>(new("nickChanged", Nick));
            DoneChangingNickname = true;
            ChangingNick = false;
        }

        public async void SetNick()
        {
            if (MainThread.IsMainThread)
            {
                string retrievedNick = MyUser.RetrieveNickFromFile(MyUser.NICK_FILENAME);
                if (retrievedNick.Equals("") || retrievedNick == null)
                {
                    string newnick = await Application.Current.MainPage.DisplayPromptAsync("Come ti chiami?",
                    "Inserisci il nome che gli altri utenti visualizzeranno", "Conferma", "Annulla",
                    "Nickname");

                    MyUser.SaveLastNickOnFile(newnick);
                    Nick = newnick;
                }
                else Nick = retrievedNick; //funzionera INotifyPropertyChanged??? ----> SI

                WeakReferenceMessenger.Default.Send<ReadyToBuildUserMessage>(new("ready"));
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
                //ANDRA TOLTO PERCHE A OGNI DISCONNESSIONE DISTUGGIAMO E RICREIAMO IL MYUSER
                else MyUser.ChangeID(Username);
            });
        }
    }
}