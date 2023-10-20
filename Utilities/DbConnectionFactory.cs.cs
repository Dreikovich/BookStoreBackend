using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace WebApiFood.Utilities
{
    public class DbConnectionFactory
    {
        private readonly IConfiguration _configuration;

        public DbConnectionFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IDbConnection CreateConnection()
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            return new SqlConnection(connectionString);
        } 

    }
}
