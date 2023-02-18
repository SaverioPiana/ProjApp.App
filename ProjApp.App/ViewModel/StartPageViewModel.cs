using ProjApp.MapEl.GPS;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging.Messages;
using CommunityToolkit.Mvvm.Messaging;
using ProjApp.Messagges;
using System.Runtime.ConstrainedExecution;
using ProjApp.Gioco;
using System.Collections.ObjectModel;

namespace ProjApp.ViewModel
{
    public partial class StartPageViewModel : ObservableObject
    {

        //questo è il json dello user e serve per mandare se stesso a ogni nuovo client che joina
        //poi basterà che ogni client in partita attraverso la funzione aggiungiGiocatoriAllaMappa
        //aggiorni i dati di posizione dei pin (gia lo fa)
        private string jsonUser;


        //messaggi
        public const string USER_HAS_JOINED_EVENT = "userHasJoinedEvent";
        public const string LOBBY_HAS_BEEN_DELETED = "lobbyHasBeenDeleted";
        public const string NICK_CHANGED = "nickChanged";
        public const string NAVIGATE_TO_MAIN_PAGE = "navigateToMainPage";
        public const string UPDATE_LIST = "UpdatePlayersList";
        //no parametri messaggio
        public const string NO_PAR = "";

        public StartPageViewModel()
        {
            while(MyUser.IsUserUpdating)
            {
                Task.Delay(40).Wait();
            } Task.Delay(10).Wait();

            DisplayCorrectUI(new(NICK_CHANGED, MyUser.user.Nickname));

            jsonUser = MyUser.CreateJsonUser(MyUser.user);

            //quando dobbiamo aggiornare la UI
            WeakReferenceMessenger.Default.Register<UIChangeAlertStartPage>(this,
                (r,m) => DisplayCorrectUI(m.Value));
            //////////////////////////////////
        }

        [ObservableProperty]
        string nick;
        [ObservableProperty]
        string codice;

        public static ObservableCollection<User> GiocatoriLobbyModifiyng = new();
        public ObservableCollection<User> GiocatoriLobby { get; set; } = GiocatoriLobbyModifiyng;

        [ObservableProperty]
        bool hasCopied = false;
        [ObservableProperty]
        bool hasToNavigate = false;
        [ObservableProperty]
        bool isCodiceVisible = false;
        [ObservableProperty, NotifyPropertyChangedFor(nameof(HasJoined))]
        bool hasCreated = false;
        [ObservableProperty, NotifyPropertyChangedFor(nameof(HasJoined))]
        bool canJoin = true;
        [ObservableProperty]
        bool entryEnabled = true;

        public bool HasJoined => (!HasCreated && !CanJoin);

        ////////////////////////  CREATING  ////////////////////////////////// 

        [RelayCommand]
        public void CreateLobby()
        {
            //SERVE PER LEVARE LA TASTIERA DALLA ENTRY, MICROSOFT ANCORA NON LO FA rip
            EntryEnabled = false;
            EntryEnabled = true;

            //NON CE ERROR HANDLING QUI
            MyUser.currPartita.CreateLobby(jsonUser);
            Codice = MyUser.currPartita.Cod_partita;
            IsCodiceVisible = true;
            HasCreated= true;
            CanJoin = false;
        }

        [RelayCommand]
        public async void AvviaPartita()
        {
            if (HasCreated)
            {
                MyUser.currPartita.StartGame();
            }
        }

        [RelayCommand]
        public void DeleteLobby()
        {
            if(HasCreated)
            {
                MyUser.currPartita.DeleteLobby();
                HasCreated= false;
                CanJoin = true;
                IsCodiceVisible = false;
            } else Application.Current.MainPage.DisplayAlert("Are u an hacker?",
                "non puoi chiudere una partita se non sei l'admin, come hai clickato sto pulsante???",
                "Non lo so sorry");
        }

        ////////////////////////////////////////////////////////////////////////


        ////////////////////////  JOINING  ////////////////////////////////// 

        [RelayCommand]
        public void JoinLobby(Entry entry)
        {
            if(CanJoin)
            { 
                //SERVE PER LEVARE LA TASTIERA DALLA ENTRY, MICROSOFT ANCORA NON LO FA rip
                EntryEnabled = false;
                EntryEnabled = true;

                if(entry.Text == null) { return ; }
                //faccio inserire il codice all'utente
                MyUser.currPartita.IfCheckThenJoin(entry.Text.ToUpper(), jsonUser);
                entry.Text = string.Empty;
            }
        }

        [RelayCommand]
        public void CopyCode()
        {
            Clipboard.Default.SetTextAsync(Codice);
            HasCopied= true;
            Task.Run(async() =>
            {
                await Task.Delay(3000);
                HasCopied = false;
            });
        }

        [RelayCommand]
        public void LeaveLobby()
        {
            if(HasJoined)
            {
                MyUser.currPartita.LeaveLobby();
                CanJoin = true;
                IsCodiceVisible = false;
            } 
            else Application.Current.MainPage.DisplayAlert("Are u an hacker?",
                "non puoi lasciare una lobby se non sei in una lobby, come hai clickato sto pulsante???",
                "Non lo so sorry");
        }

        ////////////////////////////////////////////////////////////////////////
        


        ////////////////////////////////////////////////////////////////////////
        //to know whenever we need to change the UI
        public class UIChangeAlertStartPage : ValueChangedMessage<UI_Event<string>>
        {
            public UIChangeAlertStartPage(string uieventType, string uieventPar) : base(new(uieventType, uieventPar))
            {
               
            }
        }

        public async Task DisplayCorrectUI(UI_Event<string> uiEvent)
        {
            switch(uiEvent.EventType)
            {
                case (USER_HAS_JOINED_EVENT):
                    Codice = uiEvent.EventParameter;
                    CanJoin = false;
                    IsCodiceVisible = true;
                    await MainThread.InvokeOnMainThreadAsync(WaitToNavigate);
                    break;

                case (LOBBY_HAS_BEEN_DELETED):
                    CanJoin = true;
                    IsCodiceVisible = false;
                    Codice = string.Empty;
                    HasCreated = false;
                    GiocatoriLobbyModifiyng.Clear();
                    break;

                case (NICK_CHANGED):
                    Nick = uiEvent.EventParameter;
                    break;

                case (NAVIGATE_TO_MAIN_PAGE):
                    if(!HasToNavigate)
                    {
                        HasToNavigate = true;
                    }
                    break;
            };
        }

        public async Task WaitToNavigate()
        {
            while(!HasToNavigate)
            {
                await Task.Delay(500);
            }
            bool g = MainThread.IsMainThread;
            await AppShell.Current.GoToAsync(nameof(MainPage), false);
            Codice = string.Empty;
            HasToNavigate = false;
        }
        ////////////////////////////////////////////////////////////////////////
    }
}
