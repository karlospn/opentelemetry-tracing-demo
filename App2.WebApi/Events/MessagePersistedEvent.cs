namespace App2.WebApi.Repository
{
    public class MessagePersistedEvent : IEvent
    {
        public string Message { get; set; }
    }

    public interface IEvent
    {

    }
}
