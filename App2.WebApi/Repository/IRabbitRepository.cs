using App2.WebApi.Events;

namespace App2.WebApi.Repository
{
    public interface IRabbitRepository
    {
        void Publish(IEvent evt);
    }
}
