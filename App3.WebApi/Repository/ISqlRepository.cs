using System.Threading.Tasks;

namespace App3.WebApi.Repository
{
    public interface ISqlRepository
    {
        Task Persist(string message);
    }
}