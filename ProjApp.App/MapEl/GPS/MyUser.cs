
using Mapsui.UI.Maui;
using Microsoft.AspNetCore.SignalR.Client;
using ProjApp.Gioco;
using System.Text.Json.Serialization;
using System.Text.Json;
using Mapsui.Nts;
using Mapsui.Extensions;
using static System.Net.Mime.MediaTypeNames;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ProjApp.MapEl.GPS
{
    public class MyUser 
    {
        private static CancellationTokenSource _cancelTokenSource;
        private static bool _isCheckingLocation;
        public static User user;
        public static Partita currPartita;
        public static bool isAdmin = false;
        public static string NICK_FILENAME = "playerNick.txt";
        
        //SignalR Parametri
        public readonly static int SEND_POS_DELAY = 3000;
        private static bool want_sendposition = true;

        //IL NICKNAME DOVRA METTERLO L UTENTE CON UNA BOX
        public static void BuildMyUser(string ID, string nick)
        {
            Location loc = RetrieveLocFromFile("lastSavedPosition.txt");
            user = new(nick, ID, loc);
            currPartita = new();
        }

        public static void ChangeNick(string newnick) 
        { 
            user.Nickname = newnick;
            SaveLastNickOnFile(newnick);
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
                        user.Position = location;
                        SaveLastPositionOnFile(location);
                        Console.WriteLine($"GET_POSITION::: Accuracy: {location.Accuracy} Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude: {location.Altitude}");
                    }

                }
                // Catch one of the following exceptions:
                //   FeatureNotSupportedException
                //   FeatureNotEnabledException
                //   PermissionException
                catch (Exception ex)
                {
                    Console.WriteLine($"EXCEPTION position not found {ex.Message}");

                }
                finally
                {
                    _isCheckingLocation = false;
                }
            
        }
        public void CancelRequest()
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


        public static async void inviaPosSignalR()
        {
            while (want_sendposition)
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
