using Mapsui;
using Mapsui.Nts.Extensions;
using Mapsui.Projections;
using Microsoft.AspNetCore.SignalR.Client;
using NetTopologySuite.Geometries;
using ProjApp.MapEl.GPS;
using ProjApp.ViewModel;
using Location = Microsoft.Maui.Devices.Sensors.Location;
using Point = NetTopologySuite.Geometries.Point;
using ProjApp;
using CommunityToolkit.Mvvm.Messaging.Messages;
using ProjApp.Messagges;

namespace ProjApp.Gioco
{
    public static class GameLogic
    {
        //costanti
        public const double DISTANZA_AVVISO = 50;
        public const double DISTANZA_INSEGUIMENTO = 30;
        public const double DISTANZA_CATTURA = 8;

        private const long TICS_PER_SECOND = 10000000;

        public const int DELAY_INIZIO_GIOCO = 90000;

        public const int TEMPO_DI_GIOCO_MINUTI = 20;

        public const double APERTURA_TENDINA_AVVISI = 325;

        public const long TIMEOUT_NOTIFICHE_AVVISO = TICS_PER_SECOND*120;

        public const long TIMEOUT_NOTIFICHE_INSEGUIMENTO = TICS_PER_SECOND*30;

        public const string INFO_PARTITA_TEXT_DEFAULT = "Informazioni partita";
        public const string INFO_PARTITA_TEXT_AVVISO = "Avviso";

        //detail text cacciatori
        public const string TEXTDETAIL_RUOLO_SEEKER = "Sei un cacciatore 🤠, aspetta che gli altri si nascondano";
        public const string TEXTDETAIL_NOTIFICA_SEEKER = "Qualcuno si nasconde nei dintorni 👀";
        public const string TEXTDETAIL_INSEGUIMENTO_SEEKER = "SEI VICINO, CATTURALO! 🏃";
        public const string TEXTDETAIL_CATTURA_SEEKER = "CATTURA COMPLETATA! 😎";
        public const string TEXTDETAIL_TANATO_SEEKER = "Sei un hacker? Non potresti tanarti da cacciatore! 👨‍💻";
        public const string TEXTDETAIL_INIZIO_SEEKER = "La partita è cominciata, che la caccia abbia inizio!";


        //detail text hiders
        public const string TEXTDETAIL_RUOLO_HIDER = "Nasconditi dai cacciatori! 😶‍🌫";
        public const string TEXTDETAIL_NOTIFICA_HIDER = "Un cacciatore è nei paraggi 👀";
        public const string TEXTDETAIL_INSEGUIMENTO_HIDER = "SCAPPA DAL CACCIATORE! 😱";
        public const string TEXTDETAIL_CATTURA_HIDER = "SEI STATO CATTURATO! 💀";
        public const string TEXTDETAIL_TANATO_HIDER = "Ti sei tanato, HAI VINTO! 🥳";
        public const string TEXTDETAIL_INIZIO_HIDER = "La partita è cominciata, non farti prendere";

        //detail text match over
        public const string TEXTDETAIL_MATCHOVER = "La partita è finita, ";

        //avvisi
        public const string AVVISO_RUOLO = "EventoRuolo";
        public const string AVVISO_NOTIFICA = "EventoNotifica";
        public const string AVVISO_INSEGUIMENTO = "EventoInseguimento";
        public const string AVVISO_CATTURA = "EventoCattura";
        public const string AVVISO_TANATO = "EventoTanato";
        public const string AVVISO_MATCH_OVER = "MatchIsOver";
        public const string AVVISO_INIZIO = "MatchIsBegan";

        //mappa per ogni giocatore quando e' stato inviato l'ultimo avviso -> da clearare per ogni start game
        public static Dictionary<string, long> UidToLastTime_AvvisoNotifica = new();
        public static Dictionary<string, long> UidToLastTime_AvvisoInseguimento = new();

        //messaggio per aprire la tendina per gli avvisi generati ad eventi per la distanza tra giocatori
        public class OpenAvvisoMessage : ValueChangedMessage<UI_Event<double>>
        {
            public OpenAvvisoMessage(UI_Event<double> value) : base(value)
            {
            }
        }


        public static async Task<List<User>> whoOutsideTheArea()
        {
            List<Coordinate> bordi = MyUser.currPartita.area.bordi;
            //bordi.Remove(bordi.First());
            //ma quante funz su liste tipo PF WOW!(ci sta pure Any che e' tipo exists)
            //List<User> UO = (List<User>)MyUser.currPartita.Players.Where( x => IsOutsideTheArea(bordi, x.Position));
            List<User> UO = new List<User>();
            foreach (User p in MyUser.currPartita.Players)
            {
                if (!IsInsideTheArea(bordi, p.Position))
                    UO.Add(p);
            }
            return UO;
        }

        // uso di "Ray Casting Method" , chat GPT in aiuto
        public static bool IsInsideTheArea(List<Coordinate> polygon, Location testPoint)
        {  
            MPoint p = SphericalMercator.FromLonLat(new(testPoint.Longitude,testPoint.Latitude));
            return IsInsideTheArea(polygon, p); ;
        }

        public static bool IsInsideTheArea(List<Coordinate> polygon, MPoint testPoint)
        {
            Coordinate c = testPoint.ToCoordinate();
            Point p = new(c);
            var area = new Polygon(new(polygon.ToArray()));
            return area.Contains(p); ;
        }

        public static bool IsInsideTheArea(List<Coordinate> polygon, Geometry polygon2)
        {
            var area = new Polygon(new(polygon.ToArray()));
            return area.Contains(polygon2); 
        }

        //LOGICA EVENTI DISTANZA E VISIBILITA PIN
        public static async Task<bool> ShouldPinBeVisible(SerializableUser receivedUser, bool previousVal, double distanceMts, bool IsHuntPossible)
        {
            bool res = previousVal;

            //stesso ruolo
            if ((receivedUser.IsCercatore && MyUser.user.IsCercatore) || (!receivedUser.IsCercatore && !MyUser.user.IsCercatore))
            {
                res = true;
            }
            //ruoli diversi
            if (!receivedUser.IsCercatore && MyUser.user.IsCercatore)
            {
                if (IsHuntPossible)
                {
                    res = await EventOnDistance(receivedUser, distanceMts, previousVal);
                }
                else return false;
            }
            if (receivedUser.IsCercatore && !MyUser.user.IsCercatore)
            {
                if (IsHuntPossible)
                {
                    res = await EventOnDistance(receivedUser, distanceMts, previousVal);
                }
                else return false;
            }

            return res;
        }

        //DA FARE
        public static async Task<bool> EventOnDistance(SerializableUser receivedUser, double distanceMts, bool previousVal)
        {
            bool res = previousVal;

            //solo una volta, evento cattura
            if (distanceMts <= DISTANZA_CATTURA)
            {
                //SAS PRESOS
                //se non è gia stato preso
                if (!receivedUser.IsPreso)
                {
                    //se non sei il cacciatore ti marchi come preso
                    if (!MyUser.user.IsCercatore && !MyUser.user.IsPreso)
                    {
                        MainPageViewModel.EventoDiGioco(MainPageViewModel.DEAD_ICON_FILENAME, MainPageViewModel.EVENTO_CATTURA);
                    }
                    //se sei il cercatore aggiungi laltro user alla lista dei tuoi presi
                    if(MyUser.user.IsCercatore)
                    {
                        MyUser.user.NicknameGiocatoriPresi.Add(receivedUser.Nickname);
                    }
                    await MainPageViewModel.ApriTendinaAvviso(APERTURA_TENDINA_AVVISI, AVVISO_CATTURA);
                }
            }
            else
            {
                //la prima volta avviso e vibrazione(a ogni cooldown del timer), poi solo visibilita true del pin
                if (distanceMts <= DISTANZA_INSEGUIMENTO)
                {
                    //SAS INSEGUIMENTO PAZZO
                    if ((DateTime.Now.Ticks - UidToLastTime_AvvisoInseguimento[receivedUser.UserID]) > TIMEOUT_NOTIFICHE_INSEGUIMENTO) //prima volta o timer scaduto -> avviso
                    {
                        GameLogic.UidToLastTime_AvvisoInseguimento[receivedUser.UserID] = DateTime.Now.Ticks;
                        await MainPageViewModel.ApriTendinaAvviso(APERTURA_TENDINA_AVVISI, AVVISO_INSEGUIMENTO);
                    }
                    //e in ogni caso -> 
                    res = true;
                }
                 else
                {
                    //avviso solo la prima volta
                    if (distanceMts <= DISTANZA_AVVISO)
                    {
                        //SAS AVVISO SILENZIOSOS
                        if ((DateTime.Now.Ticks - UidToLastTime_AvvisoNotifica[receivedUser.UserID]) > TIMEOUT_NOTIFICHE_INSEGUIMENTO) //prima volta o timer scaduto -> avviso
                        {
                            GameLogic.UidToLastTime_AvvisoNotifica[receivedUser.UserID] = DateTime.Now.Ticks;
                            await MainPageViewModel.ApriTendinaAvviso(APERTURA_TENDINA_AVVISI, AVVISO_NOTIFICA);
                        }
                        //e in ogni caso -> 
                        res = false;
                    }
                }
            }
            return res;
        }

    }
}
