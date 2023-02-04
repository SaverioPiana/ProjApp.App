using Android.Speech.Tts;
using CommunityToolkit.Mvvm.Messaging;
using Mapsui;
using Mapsui.Extensions;
using Mapsui.Projections;
using Mapsui.UI;
using Mapsui.UI.Maui;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Primitives;
using NetTopologySuite.Geometries;
using ProjApp.MapEl;
using ProjApp.MapEl.GPS;
using ProjApp.MapEl.Serializable;
using ProjApp.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static ProjApp.ViewModel.StartPageViewModel;
using Position = Mapsui.UI.Maui.Position;

namespace ProjApp.Gioco
{
    public class Partita
    {
        private string cod_partita;
        
        private int tap_counter = 0;
        public Tana tana;

        private string meJson;
        
        public IList<User> Players { get ; set; }
        public AreaGiocabile area { get; set; }  
        
        public string Cod_partita { get { return cod_partita; } set { cod_partita = value; } }

        public Partita()
        {
            area = new();
            Players = new List<User>();
            Connessione.con.On("InvalidID", InvalidID);
            Connessione.con.On<string,string>("JoinLobby", (lid, jsonUser) => JoinLobby(lid, jsonUser));
            Connessione.con.On<string,string,bool>("AddUserFromServer", (jsonUser,clientId, hasToSend) => AddUserFromServer(jsonUser, clientId, hasToSend));
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
            Connessione.con.InvokeAsync("JoinLobby", lid, jsonUser);
            //joino la lobby con quell'id
            Cod_partita = lid;
            MyUser.AddToCurrPartita(MyUser.user);
            Task.Run(IsGameStarted);
            Task.Run(MyUser.inviaPosSignalR);

            WeakReferenceMessenger.Default.Send<UserHasJoinedAlert>(new(MyUser.currPartita.Cod_partita));

            //se sei l'admin crei l'area
            if (MyUser.isAdmin)
            {
                OurMapController.mapView.SingleTap += creaPin;
                //event subscription
                GameLogic.UsersOutside += onUserOutside;
            }
            //senno' ricevi gli oggetti di gioco
            else
            {
                Task.Run(riceviOggettiDiGioco);
            }
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
            OurMapController.preMatchPins.Add(userPin);

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


        
        public void InvalidID() {
            //se il codice non e' valido notifico l'utente
            MainThread.BeginInvokeOnMainThread(()=>
            Application.Current.MainPage.DisplayAlert("Ops...", 
                "Pare proprio non esista una partita con quell'ID", "OK")
            );
        }

        public void IfCheckThenJoin(string lid, string jsonUser)
        {
            //controllo sul server se la lobby esiste
            Connessione.con.InvokeAsync("IfCheckThenJoin", lid, jsonUser);
        }

        public void LeaveLobby()
        {
            Connessione.con.InvokeAsync("LeaveLobby", MyUser.currPartita.Cod_partita);
        }

        public void DeleteLobby()
        {
            Connessione.con.InvokeAsync("DeleteLobby", MyUser.currPartita.Cod_partita);
            MyUser.isAdmin = false;
        }

        public void StartGame()
        {
            if(MyUser.isAdmin) 
            {
                Connessione.con.InvokeAsync("StartGame", arg1: cod_partita);
            } else { Debug.WriteLine("????????????????????? //CANNOT START GAME IF NOT ADMIN// ????????????????????????"); }
        }

        private void IsGameStarted()
        {
            Connessione.con.On<bool>("GameStarted", (isCacciatore) =>
            {
                if (isCacciatore)
                {
                    Console.WriteLine("GameStarted message from server, SEI IL CACCIATORE");
                    MyUser.user.IsCercatore = true;
                    Shell.Current.GoToAsync(nameof(MainPage));
                }
                else
                {
                    Console.WriteLine("GameStarted message from server, non sei il cacciatore");
                    Shell.Current.GoToAsync(nameof(MainPage));
                }
            });
        }
        //event handler
        private void onUserOutside(object sender, List<User> UO)
        {
            tap_counter = 0;
            area = new();
            OurMapController.mapView.Map.Layers.Remove(
                OurMapController.mapView.Map.Layers.Single(x => x.Name.Equals( AreaGiocabile.NOME_LAYER_AREA ))
                );
            StringBuilder s = new();
            UO.ForEach(u => s.Append(u.UserID + ", "));
            Console.WriteLine($"{s} sono fuori dall' area");
        }
        private void creaPin(object sender, Mapsui.UI.TappedEventArgs e)
        {
            tap_counter++;
            // Get the coordinates of the tap
            MPoint worldPosition = OurMapController.mapView.Viewport.ScreenToWorld(e.ScreenPosition);
            switch (tap_counter){
                case <6:
                    area.puntoBordo(worldPosition);
                    break;
                case 6:
                    area.creaArea();
                    GameLogic.whoOutsideTheArea();
                    break;
                case 7:
                    this.tana = new(worldPosition);
                    if (GameLogic.IsInsideTheArea(area.bordi, worldPosition))
                    {
                        tana.drawArea();
                        inviaOggettiDiGioco();
                        
                        StartGame();
                    }
                    else tap_counter--;
                    break;
            };
 
        }

        private void riceviOggettiDiGioco()
        {
            Connessione.con.On<string, string>("RiceviOggettiDiGioco", (coordarea , tana) =>
           {
               SerializableCoordinate[] ca = JsonSerializer.Deserialize<SerializableCoordinate[]>(coordarea);
               SerializableCoordinate ct = JsonSerializer.Deserialize<SerializableCoordinate>(tana);
               area.drawArea(ca);
               this.tana = new(ct);
           });
        }

        private void inviaOggettiDiGioco()
        {
            SerializableCoordinate[] gioco = SerializableCoordinate.fromCoordinateArray(area.bordi.ToArray());
            SerializableCoordinate tana = new(this.tana.position.Longitude ,this.tana.position.Latitude);


            string jcord = JsonSerializer.Serialize<SerializableCoordinate[]>
                ( gioco , new JsonSerializerOptions
                { 
                    NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
                    PropertyNameCaseInsensitive = true
                });

            string jtana = JsonSerializer.Serialize<SerializableCoordinate>
                (tana, new JsonSerializerOptions
                {
                    NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
                    PropertyNameCaseInsensitive = true
                });

            Connessione.con.InvokeAsync("InviaOggettiDiGioco", 
                arg1: MyUser.currPartita.Cod_partita, 
                arg2: jcord, 
                arg3: jtana);
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
