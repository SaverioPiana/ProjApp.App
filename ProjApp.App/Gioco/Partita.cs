using CommunityToolkit.Mvvm.Messaging;
using Mapsui;
using Mapsui.UI.Maui;
using Microsoft.AspNetCore.SignalR.Client;
using ProjApp.MapEl;
using ProjApp.MapEl.GPS;
using ProjApp.MapEl.Serializable;
using ProjApp.ViewModel;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using static ProjApp.ViewModel.StartPageViewModel;

namespace ProjApp.Gioco
{
    public class Partita
    {
        private object lockPlayers = new object();

        private static List<IDisposable> serverRegistrations = new();
        private string cod_partita;
        
        //private int tap_counter = 0;
        public Tana tana;

        private string meJson;
        private ObservableCollection<User> players;


        public ObservableCollection<User> Players
        {
            get
            {
                lock (lockPlayers)
                {
                    return players;
                }
            }
            set
            {
                lock (lockPlayers)
                {
                    players = value;
                }
            }
        }

        public AreaGiocabile area { get; set; }  
        
        public string Cod_partita { get { return cod_partita; } set { cod_partita = value; } }

        public Partita()
        {
            area = new();
            Players = new ObservableCollection<User>();
            serverRegistrations.Add(
                Connessione.con.On<string>("ServerError", (errormsg) => ServerError(errormsg))
            );
            serverRegistrations.Add(
                Connessione.con.On<string, string>("JoinLobby", (lid, jsonUser) => JoinLobby(lid, jsonUser))
            );
            serverRegistrations.Add(
                Connessione.con.On<string, string, bool>("AddUserFromServer", (jsonUser, clientId, hasToSend) => AddUserFromServer(jsonUser, clientId, hasToSend))
            );

            meJson = MyUser.CreateJsonUser(MyUser.user);
        }

        public void CreateLobby(string jsonUser)
        {
            MyUser.isAdmin = true;
            cod_partita = CreateCode();

            Connessione.con.InvokeAsync("CreateLobby", arg1: cod_partita);
            this.IfCheckThenJoin(Cod_partita, jsonUser);
            Console.WriteLine($"Lobby Creata con codice {cod_partita}");

        }

        public void JoinLobby(string lid, string jsonUser)
        {
            Connessione.con.InvokeAsync("JoinLobby", lid, jsonUser, MyUser.user.UserID);
            //joino la lobby con quell'id
            Cod_partita = lid;
            MyUser.AddToCurrPartita(MyUser.user);
            Task.Run(IsGameStarted);
            Task.Run(OnDeletedLobby);
            Task.Run(RemoveUserFromListAndPins);
            
            WeakReferenceMessenger.Default.Send<UIChangeAlertStartPage>(new(USER_HAS_JOINED_EVENT, MyUser.currPartita.Cod_partita));
        }


        public void RemoveUserFromListAndPins()
        {
            Connessione.con.On<string>("UserLeft", (userId) =>
            {
                IList<User> copy = MyUser.currPartita.Players;
                foreach (User p in copy)
                {
                    if (p.UserID.Equals(userId))
                    {
                        MyUser.currPartita.Players.Remove(p);
                        MainPageViewModel.PreMatchPins.Remove(p.UserPin);
                        break;
                    }
                }
            });
        }


        //SE viene chiamata dall'ultimo client joinato allora TU devi mandargli il tuo user
        //SENNO' se viene chiamata da altri VERSO l'ultimo joinato NON devi mandare niente
        //(ti hanno aggiunto prima con questa funzione)

        //hasToSend -> true se vuoi mandare il tuo user DAL CLIENT DA CUI HAI RICEVUTO LA CHIAMATA
        public void AddUserFromServer(string jsonUser, string clientId, bool hasToSend) 
        {
            SerializableUser user = JsonSerializer.Deserialize<SerializableUser>(jsonUser);

            Pin userPin = new Pin()
            {
                Label = user.UserID,
                Position = new(0,0),
                Type = PinType.Icon,
                Icon = user.UserIcon,
                Scale = 0.4F
            };

            //AGGIUNGO IL PIN AI PIN PREPARTITA (verranno caricati quando crei la mapview)
            MainPageViewModel.PreMatchPins.Add(userPin);

            //creo loggetto user e lo aggiungo alla lista dei players nella partita

            User joineduser = new(user.Nickname, user.UserID, new(0, 0))
            {
                UserPin = userPin
            };

            MyUser.AddToCurrPartita(joineduser);

            //mando il mio user al client appena joinato (clientId) se non sono l'ultimo joinato
            if (hasToSend)
            {
                Connessione.con.InvokeAsync("SendToLastJoined", clientId, meJson);
            }
        }

        public void ServerError(string errormsg) {
            //se il codice non e' valido notifico l'utente
            MainThread.BeginInvokeOnMainThread(()=>
            Application.Current.MainPage.DisplayAlert("Ops...", 
                errormsg, "OK")
            );
        }

        public void IfCheckThenJoin(string lid, string jsonUser)
        {
            //controllo sul server se la lobby esiste
            Connessione.con.InvokeAsync("IfCheckThenJoin", lid, jsonUser);
        }

        public void LeaveLobby()
        {
            Connessione.con.InvokeAsync("LeaveLobby", MyUser.currPartita.Cod_partita, MyUser.user.UserID);
            LobbyParamReset();
        }

        public void DeleteLobby()
        {
            Connessione.con.InvokeAsync("DeleteLobby", MyUser.currPartita.Cod_partita);
            LobbyParamReset();
        }

        public void OnDeletedLobby()
        {
            Connessione.con.On("DeletedLobby", () => 
            {
                //mando un messaggio alla startpage per cambiare la UI
                WeakReferenceMessenger.Default.Send<UIChangeAlertStartPage>(new(LOBBY_HAS_BEEN_DELETED, NO_PAR));
                LobbyParamReset();
            });
        }

        private void LobbyParamReset()
        {
            MyUser.user.IsPreso = false;
            MyUser.user.IsCercatore = false;
            MyUser.isAdmin = false;
            MyUser.SEND_POSITION = false;
            //RIMUOVO I PIN DAI PIN PREPARTITA
            GameLogic.UidToLastTime_AvvisoNotifica.Clear();
            GameLogic.UidToLastTime_AvvisoInseguimento.Clear();
            MainPageViewModel.PreMatchPins.Clear();
            MyUser.currPartita.Players.Clear();
        }

        public void StartGame()
        {
            if(MyUser.isAdmin) 
            {
                Connessione.con.InvokeAsync("StartGame", arg1: Cod_partita);
            } else { Debug.WriteLine("????????????????????? //CANNOT START GAME IF NOT ADMIN// ????????????????????????"); }
        }

        private void IsGameStarted()
        {
            Connessione.con.On<bool>("GameStarted", (isCacciatore) =>
            {
                MyUser.SEND_POSITION = true;
                Task.Run(MyUser.inviaPosSignalR);
                if (isCacciatore)
                {
                    Console.WriteLine("GameStarted message from server, SEI IL CACCIATORE");
                    MyUser.user.IsCercatore = true;
                }
                else
                {
                    Console.WriteLine("GameStarted message from server, non sei il cacciatore");
                }
                WeakReferenceMessenger.Default.Send<UIChangeAlertStartPage>(new(NAVIGATE_TO_MAIN_PAGE, NO_PAR));
            });
        }

        public void DisposeServerRegistrations()
        {
            foreach(var s in serverRegistrations)
            {
                s.Dispose();
            }
        }

        //crea un codice hash per la partita usando data e ora e userID
        private static string CreateCode()
        {
            StringBuilder sb = new(DateTime.Now.ToString());
            sb.Append(MyUser.user.UserID);
            string cod = sb.ToString();
            int hashcode = Math.Abs(cod.GetHashCode());

            // convert the hashcode to a base-26 number, where each digit is represented by a letter of the alphabet
            string base26 = "";
            while (hashcode > 0)
            {
                int digit = hashcode % 26;
                //usa addizione codice ASCII
                char letter = (char)('A' + digit);
                base26 = letter + base26;
                hashcode = hashcode / 26;
            }

            // take the first 5 digits of the base-26 representation as your unique code
            string uniqueCode = base26.Substring(0, 5);

            return uniqueCode; // prints the unique code
        }
    }
}
