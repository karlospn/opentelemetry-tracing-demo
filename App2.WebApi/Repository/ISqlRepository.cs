using System.Threading.Tasks;

namespace App2.WebApi.Repository
{
    public interface ISqlRepository
    {
        Task Persist(string message);
    }
}