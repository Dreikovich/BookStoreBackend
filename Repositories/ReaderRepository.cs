using System.ComponentModel;
using System.Data.SqlClient;
using WebApiFood.Utilities;

namespace WebApiFood.Repositories
{
    public class ReaderRepository
    {
        private readonly DbConnectionFactory _dbConnectionFactory;

        public ReaderRepository(DbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }
        public int GetReaderIdByName(string name)
        {
            var sqlQuery = "SELECT readerId FROM readers WHERE readerName=@name";
            using var connection = _dbConnectionFactory.CreateConnection();
            connection.Open();
            using var command = new SqlCommand(sqlQuery, (SqlConnection)connection);
            command.Parameters.AddWithValue("@name", name);
            var readerId = (int?)command.ExecuteScalar();
            if (readerId.HasValue)
            {
                connection.Close();
                return readerId.Value;
            }
            else
            {
                throw new InvalidAsynchronousStateException("Reader not found");
            }
        }
    }
}
