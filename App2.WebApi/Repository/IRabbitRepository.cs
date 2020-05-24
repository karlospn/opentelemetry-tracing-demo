using System.Threading.Tasks;

namespace App2.WebApi.Repository
{
    public interface IRabbitRepository
    {
        void Publish(IEvent evt);
    }
}
