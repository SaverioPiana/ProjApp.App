﻿using ProjApp.MapEl.GPS;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Globalization;
using Microsoft.Maui.Controls;
using System.Text.Json.Serialization;
using System.Text.Json;
using ProjApp.Gioco;
using CommunityToolkit.Mvvm.Messaging.Messages;
using CommunityToolkit.Mvvm.Messaging;

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
            while (!MyUser.IsUserBuilt)
            {
                Task.Delay(50).Wait();
            }

            //quando l'utente joina riceveremo un messaggio e aggiorneremo il codice lobby
            WeakReferenceMessenger.Default.Register<UserHasJoinedAlert>(this,
                (r,m) => PutCurrentLobbyCodeWhenUserJoins(m.Value));
            ///////////////////////////
            
            Nick = MyUser.user.Nickname;
            jsonUser = MyUser.CreateJsonUser(MyUser.user);
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
            //faccio inserire il codice all'utente
            MyUser.currPartita.IfCheckThenJoin(entry.Text, jsonUser);
            IsCodiceVisible= true;
            HasCreated = false;
            CanJoin= false;
        }

        [RelayCommand]
        public Task CopyCode()
        {
            Clipboard.Default.SetTextAsync(Codice);
            HasCopied= true;
            Task.Run(async() =>
            {
                await Task.Delay(3000);
                HasCopied = false;
            });
            return Task.CompletedTask;
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
        //to know when the user joins a lobby
        public class UserHasJoinedAlert : ValueChangedMessage<string>
        {
            public UserHasJoinedAlert(string value) : base(value)
            {
            }
        }

        public void PutCurrentLobbyCodeWhenUserJoins(string code)
        {
            Codice = code;
        }
        ////////////////////////////////////////////////////////////////////////
    }
}
