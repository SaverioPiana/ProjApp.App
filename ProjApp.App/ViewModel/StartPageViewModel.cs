using ProjApp.MapEl.GPS;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Globalization;
using Microsoft.Maui.Controls;
using System.Text.Json.Serialization;
using System.Text.Json;
using ProjApp.Gioco;

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
            while (!MyUser.isUserBuilt)
            {
                Task.Delay(50).Wait();
            }
            Nick = MyUser.user.Nickname;
            jsonUser = MyUser.CreateJsonUser(MyUser.user);
        }

        [ObservableProperty]
        string codice;
        [ObservableProperty]
        bool isCodiceVisible = false;
        [ObservableProperty]
        bool hasCreated = false;
        [ObservableProperty]
        bool canJoin = true;
        [ObservableProperty]
        string nick;

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
        public void DeleteLobby()
        {
            if(HasCreated)
            {
                MyUser.currPartita.DeleteLobby();
                HasCreated= false;
                CanJoin = true;
                IsCodiceVisible = false;
            }
        }

        [RelayCommand]
        public void AvviaPartita()
        {
            if (HasCreated)
            {
                MyUser.currPartita.StartGame();
            }
            else Application.Current.MainPage.DisplayAlert("Are u an hacker?",
                "non puoi avviare una partita se non sei l'admin, come hai clickato sto pulsante???",
                "Non lo so sorry");
        }

        ////////////////////////////////////////////////////////////////////////


        ////////////////////////  JOINING  ////////////////////////////////// 

        [RelayCommand]
        public void JoinLobby(Entry entry)
        {
            //faccio inserire il codice all'utente
            MyUser.currPartita.IfCheckThenJoin(entry.Text, jsonUser);
            HasCreated = false;
            CanJoin= false;
        }

        [RelayCommand]
        public Task CopyCode()
        {
            Clipboard.Default.SetTextAsync(Codice);
            
            return Task.CompletedTask;
        }


        ////////////////////////////////////////////////////////////////////////

    }
}
