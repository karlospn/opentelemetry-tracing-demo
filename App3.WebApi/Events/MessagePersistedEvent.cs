namespace App3.WebApi.Events
{
    public class MessagePersistedEvent : IEvent
    {
        public string Message { get; set; }
    }

    public interface IEvent
    {

    }
}
