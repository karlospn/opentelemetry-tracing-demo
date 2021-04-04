using System.Data.SqlClient;
using System.Threading.Tasks;

namespace App2.WebApi.Repository
{
    public class SqlRepository : ISqlRepository
    {
        public async Task Persist(string message)
        {
            await using var conn = new SqlConnection("server=localhost;user id=sa;password=Pass@Word1;");
            await conn.OpenAsync();

            //Do something more complex
            await using var cmd = new SqlCommand("SELECT GETDATE()", conn);
            var res = await cmd.ExecuteScalarAsync();
        }
    }
}
