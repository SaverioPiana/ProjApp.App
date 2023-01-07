using Mapsui;
using Mapsui.Extensions;
using Mapsui.Projections;
using Mapsui.UI;
using Mapsui.UI.Maui;
using Microsoft.AspNetCore.SignalR.Client;
using NetTopologySuite.Geometries;
using ProjApp.MapEl;
using ProjApp.MapEl.GPS;
using ProjApp.MapEl.Serializable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Position = Mapsui.UI.Maui.Position;

namespace ProjApp.Gioco
{
    internal class Partita
    {
        private IList<User> players;
        private string cod_partita;
        private bool giocoInCorso = false;


        //temp
        private int tap_counter = 0;
        AreaGiocabile area = new();
        Tana tana;
        //


        public IList<User> Players { get; set; }
        public bool GiocoInCorso { get; set; }
        public string Cod_partita { get { return cod_partita; } set { cod_partita = value; } }

        private readonly HubConnection _connection;

        public Partita(HubConnection con)
        {
            _connection = con;
            players = new List<User>();
            cod_partita = "SASSO";
        }

        public void CreateLobby()
        {
            MyUser.isAdmin = true;
            _connection.InvokeAsync("CreateLobby", arg1: cod_partita);
            this.JoinLobby(cod_partita);
            Console.WriteLine($"Lobby Creata con codice {cod_partita}");

        }

        public void JoinLobby(string lid)
        {
            _connection.InvokeAsync("JoinLobby", lid);
            MyUser.currPartita = lid;
            Task.Run(() => isGameStarted());
            Task.Run(() => MyUser.inviaPosSignalR());

            //creazione area di gioco
            if (MyUser.isAdmin)
                OurMapController.mapView.SingleTap += creaPin;
            else Task.Run(() => riceviOggettiDiGioco());
                
        }

        public void LeaveLobby()
        {
            _connection.InvokeAsync("LeaveLobby", MyUser.currPartita);
            MyUser.isAdmin = false;
        }
        public void StartGame()
        {
            _connection.InvokeAsync("StartGame", arg1: cod_partita);
            
        }

        private void isGameStarted()
        {
            Connessione.con.On<bool>("GameStarted", (isCacciatore) =>
            {
                if (isCacciatore)
                {
                    Console.WriteLine("GameStarted message from server, SEI IL CACCIATORE");
                }
                else
                {
                    Console.WriteLine("GameStarted message from server, non sei il cacciatore");
                }
            });
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
                    break;
                case 7:
                    this.tana = new(worldPosition);
                    inviaOggettiDiGioco();
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
                arg1: MyUser.currPartita, 
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
