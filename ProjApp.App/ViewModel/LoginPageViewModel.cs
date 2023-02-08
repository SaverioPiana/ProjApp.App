﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using ProjApp.MapEl.GPS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ProjApp.ViewModel.ProfilePageViewModel;



namespace ProjApp.ViewModel
{
    public partial class LoginPageViewModel : ObservableObject
    {

        private static bool FIRST_TIME_LOGGING = true;

        public LoginPageViewModel() 
        { 
            Username= string.Empty;
            Password= string.Empty;
        }

        [ObservableProperty] 
        private string username;
        [ObservableProperty] 
        private string password;

        private bool succesfullLogin = true; ////////per ora true semrpe



        [RelayCommand]
        Task NavigateToStartPage() {
            if (succesfullLogin)
            {
                if (Shell.Current == null)
                {
                    Application.Current.MainPage = new AppShell();
                }
                Shell.Current.GoToAsync($"//{nameof(ProfilePage)}");


                //SOLO LA PRIMA VOLTA (da login -> profile)
                //passiamo lo username alla pagina profilo solo se ha gia interagito con lo user per il nickmname
                if (FIRST_TIME_LOGGING)
                {
                    WeakReferenceMessenger.Default.Register<ReadyToBuildUserMessage>(this, (r, m) =>
                    {
                        WeakReferenceMessenger.Default.Send(new BuildUserMessage(Username));
                    });
                    FIRST_TIME_LOGGING = false;
                }
                else //la prifle page è gia stata creata ed è pronta a ricevere il nuovo username senza aspettare
                {
                    WeakReferenceMessenger.Default.Send(new BuildUserMessage(Username));
                }

            }
            return Task.CompletedTask;
        }
    };

    internal class ReadyToBuildUserMessage : ValueChangedMessage<string>
    {
        public ReadyToBuildUserMessage(string value) : base(value)
        {
        }
    }
}
