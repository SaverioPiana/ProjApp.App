namespace ProjApp.Messagges
{
    public class UI_Event<T>
    {
        private string eventType;
        private T eventParameter;

        public string EventType { get { return eventType; } }
        public T EventParameter { get { return eventParameter; } }

        public UI_Event(string eventType, T eventParameter)
        {
            this.eventType = eventType;
            this.eventParameter = eventParameter;
        }
    }
}
