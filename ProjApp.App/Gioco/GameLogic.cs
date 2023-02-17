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
        public const double DISTANZA_AVVISO = 40;
        public const double DISTANZA_INSEGUIMENTO = 20;
        public const double DISTANZA_CATTURA = 5;

        private const long TICS_PER_SECOND = 10000000;

        public const int DELAY_INIZIO_GIOCO = 180000;

        private const double APERTURA_TENDINA_AVVISI = 325;

        public const long TIMEOUT_NOTIFICHE_AVVISO = TICS_PER_SECOND*120;

        public const long TIMEOUT_NOTIFICHE_INSEGUIMENTO = TICS_PER_SECOND*30;

        public const string INFO_PARTITA_TEXT_DEFAULT = "Informazioni partita";
        public const string INFO_PARTITA_TEXT_AVVISO = "Avviso";

        //detail text cacciatori
        public const string TEXTDETAIL_NOTIFICA_SEEKER = "Qualcuno si nasconde nei dintorni";
        public const string TEXTDETAIL_INSEGUIMENTO_SEEKER = "SEI VICINO, CATTURALO!";
        public const string TEXTDETAIL_CATTURA_SEEKER = "Cattura completata! :)";

        //detail text hiders
        public const string TEXTDETAIL_NOTIFICA_HIDER = "Un cacciatore è nei paraggi";
        public const string TEXTDETAIL_INSEGUIMENTO_HIDER = "SCAPPA DAL CACCIATORE!";
        public const string TEXTDETAIL_CATTURA_HIDER = "Sei stato catturato! :(";

        //avvisi
        public const string AVVISO_NOTIFICA = "EventoNotifica";
        public const string AVVISO_INSEGUIMENTO = "EventoInseguimento";
        public const string AVVISO_CATTURA = "EventoCattura";

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
        public static async Task<bool> ShouldPinBeVisible(string uid, bool isPinCercatore, bool previousVal, double distanceMts, bool IsHuntPossible)
        {
            bool res = previousVal;

            //stesso ruolo
            if ((isPinCercatore && MyUser.user.IsCercatore) || (!isPinCercatore && !MyUser.user.IsCercatore))
            {
                res = true;
            }
            //ruoli diversi
            if (!isPinCercatore && MyUser.user.IsCercatore)
            {
                if (IsHuntPossible)
                {
                    res = await EventOnDistance(uid, distanceMts, previousVal);
                }
                else return false;
            }
            if (isPinCercatore && !MyUser.user.IsCercatore)
            {
                if (IsHuntPossible)
                {
                    res = await EventOnDistance(uid, distanceMts, previousVal);
                }
                else return false;
            }

            return res;
        }

        //DA FARE
        public static async Task<bool> EventOnDistance(string uid, double distanceMts, bool previousVal)
        {
            bool res = previousVal;

            //avviso solo la prima volta
            if (distanceMts <= DISTANZA_AVVISO)
            {
                //SAS AVVISO SILENZIOSOS
                if ((DateTime.Now.Ticks - UidToLastTime_AvvisoNotifica[uid]) > TIMEOUT_NOTIFICHE_INSEGUIMENTO) //prima volta o timer scaduto -> avviso
                {
                    GameLogic.UidToLastTime_AvvisoNotifica[uid] = DateTime.Now.Ticks;
                    await MainPageViewModel.ApriTendinaAvviso(APERTURA_TENDINA_AVVISI, AVVISO_NOTIFICA);
                }
                //e in ogni caso -> 
                res = false;
            }
            //la prima volta avviso e vibrazione(a ogni cooldown del timer), poi solo visibilita true del pin
            if (distanceMts <= DISTANZA_INSEGUIMENTO)
            {
                //SAS INSEGUIMENTO PAZZO
                if ((DateTime.Now.Ticks - UidToLastTime_AvvisoInseguimento[uid]) > TIMEOUT_NOTIFICHE_INSEGUIMENTO) //prima volta o timer scaduto -> avviso
                {
                    GameLogic.UidToLastTime_AvvisoInseguimento[uid] = DateTime.Now.Ticks;
                    await MainPageViewModel.ApriTendinaAvviso(APERTURA_TENDINA_AVVISI, AVVISO_INSEGUIMENTO);
                }
                //e in ogni caso -> 
                res = true;
            }
            //solo una volta, evento cattura
            if (distanceMts <= DISTANZA_CATTURA)
            {
                User otherUser = null;
                foreach (User u in MyUser.currPartita.Players)
                {
                    if (u.UserID == uid)
                    {
                        otherUser = u;
                    }
                }
                //SAS PRESOS
                //se non è gia stato preso
                if(!otherUser.isPreso)
                {
                    //prendilo
                    if (MyUser.user.IsCercatore)
                    {
                        await Connessione.con.InvokeAsync("GiocatorePreso", MyUser.currPartita.Cod_partita, uid);
                    }
                    await MainPageViewModel.ApriTendinaAvviso(APERTURA_TENDINA_AVVISI, AVVISO_CATTURA);
                }
            }

            return res;
        }

    }
}
