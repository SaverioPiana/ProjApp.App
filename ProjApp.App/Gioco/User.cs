using Mapsui.Extensions;
using Mapsui.UI.Maui;
using Microsoft.Maui.Graphics.Platform;
using ProjApp.MapEl.GPS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using Android.Runtime;

namespace ProjApp.Gioco
{
    public class User
    {
        public string Nickname {  get; private set; }
        public string UserID { get; private set; }
        public Position position { get; set; }
        public Pin pin;
        byte[] UserIcon = ReadResource("pinicon.png");
        
        public User(string nickname, string userID, Position posizione, MapView mv)
        {
            Nickname = nickname;
            UserID = userID;
            this.position = posizione;
            this.pin = new Pin(mv)
            {
                Icon = UserIcon,
                Label = UserID,
                Position = posizione
            };
        }

        //da far fungere
        public static byte[] ReadResource(String filename)
        {
            byte[] result;
            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = assembly.GetManifestResourceNames()
                                .Single(str => str.EndsWith(filename));
            if (resourceName == null)
            {
                Console.WriteLine("///////////////////////" +
                    "IL NOME DEL FILE O IL FILE NON ESISTONO" +
                    "/////////////////////////");
                throw new ArgumentNullException();
            }
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    result = (byte[])memoryStream.ToArray();
                }
            }
            return result;
        }
    }
}
