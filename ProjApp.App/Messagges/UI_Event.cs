using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjApp.Messagges
{
    public class UI_Event
    {
        private string eventType;
        private string eventParameter;

        public string EventType { get { return eventType; } }
        public string EventParameter { get { return eventParameter; } }

        public UI_Event(string eventType, string eventParameter)
        {
            this.eventType = eventType;
            this.eventParameter = eventParameter;
        }
    }
}
