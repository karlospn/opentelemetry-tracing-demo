namespace App2.WebApi.Events
{
    public class MessagePersistedEvent : IEvent
    {
        public string Message { get; set; }
    }

    public interface IEvent
    {

    }
}
