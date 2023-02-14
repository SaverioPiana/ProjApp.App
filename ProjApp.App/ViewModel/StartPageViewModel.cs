using ProjApp.MapEl.GPS;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging.Messages;
using CommunityToolkit.Mvvm.Messaging;
using ProjApp.Messagges;

namespace ProjApp.ViewModel
{
    public partial class StartPageViewModel : ObservableObject
    {

        //questo è il json dello user e serve per mandare se stesso a ogni nuovo client che joina
        //poi basterà che ogni client in partita attraverso la funzione aggiungiGiocatoriAllaMappa
        //aggiorni i dati di posizione dei pin (gia lo fa)
        private string jsonUser;

        public StartPageViewModel()
        {
            while(MyUser.IsUserUpdating)
            {
                Task.Delay(40).Wait();
            } Task.Delay(10).Wait();

            DisplayCorrectUI(new("nickChanged", MyUser.user.Nickname));

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
        [ObservableProperty]
        bool hasCopied = false;
        [ObservableProperty]
        bool isCodiceVisible = false;
        [ObservableProperty, NotifyPropertyChangedFor(nameof(HasJoined))]
        bool hasCreated = false;
        [ObservableProperty, NotifyPropertyChangedFor(nameof(HasJoined))]
        bool canJoin = true;
        
                                             
        public bool HasJoined => (!HasCreated && !CanJoin) ;

        private async Task NavigateToMainPage() => await AppShell.Current.GoToAsync(nameof(MainPage), false);


        ////////////////////////  CREATING  ////////////////////////////////// 

        [RelayCommand]
        public void CreateLobby()
        {
            //NON CE ERROR HANDLING QUI
            MyUser.currPartita.CreateLobby(jsonUser);
            Codice = MyUser.currPartita.Cod_partita;
            IsCodiceVisible = true;
            HasCreated= true;
            CanJoin = false;
        }

        [RelayCommand]
        public async Task AvviaPartita()
        {
            if (HasCreated)
            {
                MyUser.currPartita.StartGame();
                await NavigateToMainPage();
            }
            else Application.Current.MainPage.DisplayAlert("Are u an hacker?",
                "non puoi avviare una partita se non sei l'admin, come hai clickato sto pulsante???",
                "Non lo so sorry");
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
                CanJoin = false;
                CanJoin = true;

                //faccio inserire il codice all'utente
                MyUser.currPartita.IfCheckThenJoin(entry.Text, jsonUser);
                
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
        public class UIChangeAlertStartPage : ValueChangedMessage<UI_Event>
        {
            public UIChangeAlertStartPage(string uieventType, string uieventPar) : base(new(uieventType, uieventPar))
            {
               
            }
        }

        public async Task DisplayCorrectUI(UI_Event uiEvent)
        {
            switch(uiEvent.EventType)
            {
                case ("userHasJoinedEvent"):
                    Codice = uiEvent.EventParameter;
                    CanJoin = false;
                    IsCodiceVisible = true;
                    break;
                case ("lobbyHasBeenDeleted"):
                    CanJoin = true;
                    IsCodiceVisible = false;
                    HasCreated = false;
                    break;
                case ("nickChanged"):
                    Nick = uiEvent.EventParameter;
                    break;
            };
        }
        ////////////////////////////////////////////////////////////////////////
    }
}
