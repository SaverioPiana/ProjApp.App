using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjApp.Gioco
{

    public class LobbyNotFoundException : Exception
    {
        public LobbyNotFoundException()
        {
        }

        public LobbyNotFoundException(string message)
            : base(message)
        {
        }

        public LobbyNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
