using ProjApp.MapEl.GPS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ProjApp.Gioco
{
    internal class Partita
    {
        private IList<User> players;
        private string cod_partita;
        private bool giocoInCorso = false;

        public IList<User> Players { get; set; }
        public bool GiocoInCorso { get; set; }
        public string Cod_partita { get; set; }

        public Partita()
        {
            players = new List<User>();
            cod_partita = CreateCode();
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
