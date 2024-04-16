using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace App3.WebApi.Repository
{
    public class SqlRepository(IConfiguration configuration) : ISqlRepository
    {
        private const string Query = "SELECT GETDATE()";

        public async Task Persist(string message)
        {
            await using var conn = new SqlConnection(configuration["SqlDbConnString"]);
            await conn.OpenAsync();

            //Do something more complex
            await using var cmd = new SqlCommand(Query, conn);
            var res = await cmd.ExecuteScalarAsync();
        }
    }
}
