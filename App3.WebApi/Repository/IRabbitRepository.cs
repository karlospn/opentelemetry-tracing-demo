using App3.WebApi.Events;

namespace App3.WebApi.Repository
{
    public interface IRabbitRepository
    {
        void Publish(IEvent evt);
    }
}
