
using Mapsui.UI.Maui;
using Microsoft.AspNetCore.SignalR.Client;
using ProjApp.Gioco;
using System.Text.Json.Serialization;
using System.Text.Json;
using Mapsui.Nts;
using Mapsui.Extensions;
using static System.Net.Mime.MediaTypeNames;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using static ProjApp.ViewModel.StartPageViewModel;
using Firebase.Auth;
using User = ProjApp.Gioco.User;

namespace ProjApp.MapEl.GPS
{
    public static class MyUser 
    {
        private static CancellationTokenSource _cancelTokenSource;
        private static bool _isCheckingLocation;
        public static User user;
        public static Partita currPartita;
        public static List<Partita> partiteRecenti = new();
        public static bool isAdmin = false;
        public static string NICK_FILENAME = "playerNick.txt";
        public static bool IsUserUpdating = false;

        private static int consecutiveChecks = 0;

        //SignalR Parametri
        public readonly static int SEND_POS_DELAY = 3000;
        public readonly static int FIND_POS_DELAY = 200;
        public static bool SEND_POSITION = false;

        //IL NICKNAME DOVRA METTERLO L UTENTE CON UNA BOX
        public static void BuildMyUser(string ID, string nick)
        {
            IsUserUpdating = true;
            Location loc = RetrieveLocFromFile("lastSavedPosition.txt");
            user = new(nick, ID, loc);
            currPartita = new();
            //serve senno alcune componenti nel codice provano ad
            //accedere allo user prima che sia stato creato
            //WeakReferenceMessenger.Default.Send<UIChangeAlertStartPage>(new("canDisplayNick", nick));
            IsUserUpdating = false;
        }

        public static void ChangeNick(string newnick) 
        {
            IsUserUpdating = true;
            user.Nickname = newnick;
            SaveLastNickOnFile(newnick);
            IsUserUpdating = false;
        }

        public static void ChangeID(string newID)
        {
            IsUserUpdating = true;
            user.UserID = newID;
            IsUserUpdating = false;
        }

        public static void AddToCurrPartita(User u)
        {
            currPartita.Players.Add(u);
        }

        public static async Task Get_Position()
        {
            try
            {

                _isCheckingLocation = true;

                GeolocationRequest request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(6));


                _cancelTokenSource = new CancellationTokenSource();

                Location location = await Geolocation.Default.GetLocationAsync(request, _cancelTokenSource.Token);

                if (location != null && location.Accuracy < 50)
                {
                    consecutiveChecks = 0;
                    user.Position = location;
                    SaveLastPositionOnFile(location);
                    Console.WriteLine($"GET_POSITION::: Accuracy: {location.Accuracy} Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude: {location.Altitude}");
                } else {
                    consecutiveChecks++;
                    if (consecutiveChecks >= 15)
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            Microsoft.Maui.Controls.Application.Current.MainPage.DisplayAlert("Ao? 🤔",
                            "Pare che il tuo GPS prenda molto male, prova ad appicciare il wi-fi",
                            "Prometto di uscire dal bunker");
                        });
                        consecutiveChecks = 0;
                    }
                }

            }
            // Catch one of the following exceptions:
            //   FeatureNotSupportedException
            //   FeatureNotEnabledException
            //   PermissionException
            catch (Exception ex)
            {
                Console.WriteLine($"EXCEPTION position not found {ex.Message}");
                await Task.Delay(200);
            }
            finally
            {
                _isCheckingLocation = false;
                await Task.Delay(FIND_POS_DELAY);
            }
            
        }
        public static void CancelPositionRequest()
        {
            if (_isCheckingLocation && _cancelTokenSource != null && _cancelTokenSource.IsCancellationRequested == false)
                _cancelTokenSource.Cancel();
        }

        //salva la posizione su un file
        private static async void SaveLastPositionOnFile(Location loc)
        {
            try
            {
                string jsonPos = JsonSerializer.Serialize<Location>(loc,
                    new JsonSerializerOptions
                    {
                        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
                        PropertyNameCaseInsensitive = true
                    });
                string targetFileName = "lastSavedPosition.txt";
                string path = System.IO.Path.Combine(FileSystem.Current.AppDataDirectory, targetFileName);
                using StreamWriter streamWriter = new StreamWriter(path, false);
                await streamWriter.WriteAsync(jsonPos);
            }
            catch (Exception e)
            {
                throw new Exception("Qualcosa e andato storto col file zi");
            }
        }

        //legge l'ultima posizione da un file
        private static Location RetrieveLocFromFile(string filename)
        {
            try
            {
                string jpos;
                string path = System.IO.Path.Combine(FileSystem.Current.AppDataDirectory, filename);
                //se non esiste il file lo creo
                if (!File.Exists(path))
                {
                    using StreamWriter streamWriter = new StreamWriter(path);
                    return new(0,0);
                }
                //poi lo leggo
                using (StreamReader sr = File.OpenText(path))
                {
                    jpos = sr.ReadToEnd();
                    Location loc;
                    if (jpos.Equals("") || jpos.Length < 1)
                    {
                        loc = new(0,0);
                    }
                    else
                    {
                        loc = JsonSerializer.Deserialize<Location>(jpos);
                    }
                    if (loc == null) loc = new(0,0);
                    return loc;
                };
            }
            catch (Exception e)
            {
                Console.WriteLine("/////////////////////Qualcosa e andato storto col file zi//////////////////");
                return new(0,0);
            }
        }
        public static async void SaveLastNickOnFile(string nick)
        {
            try
            {
                string targetFileName = "playerNick.txt";
                string path = System.IO.Path.Combine(FileSystem.Current.AppDataDirectory, targetFileName);
                using StreamWriter streamWriter = new StreamWriter(path, false);
                await streamWriter.WriteAsync(nick);
            }
            catch (Exception e)
            {
                throw new Exception("Qualcosa e andato storto col file zi");
            }
        }
        public static string RetrieveNickFromFile(string filename)
        {
            try
            {
                string nick;
                string path = System.IO.Path.Combine(FileSystem.Current.AppDataDirectory, filename);
                //se non esiste il file lo creo  
                if (!File.Exists(path))
                {
                    using StreamWriter streamWriter = new StreamWriter(path);
                    return "";
                }
                //poi lo leggo
                using (StreamReader sr = File.OpenText(path))
                {
                    nick = sr.ReadToEnd();
                    if (nick == null) nick = "";
                    return nick;
                };
            }
            catch (Exception e)
            {
                Console.WriteLine("/////////////////////Qualcosa e andato storto col file zi//////////////////");
                return "";
            }
        }


        public static async Task inviaPosSignalR()
        {
            while(SEND_POSITION)
            {   
                if (Connessione.con.State.Equals(HubConnectionState.Connected))
                {
                    string jsonUser = CreateJsonUser(user);

                    await Connessione.con.InvokeAsync("SendPosition",
                            arg1: jsonUser,
                            //Codice lobby
                            arg2: currPartita.Cod_partita);
                }
                await Task.Delay(SEND_POS_DELAY);
            }
        }

        public static string CreateJsonUser(User u)
        {
            return JsonSerializer.Serialize<User>(u,
                    new JsonSerializerOptions
                    {
                        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
                        PropertyNameCaseInsensitive = true
                    });
        }
    }
}
