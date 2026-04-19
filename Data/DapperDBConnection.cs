using Microsoft.Data.SqlClient;
using System.Data;

namespace SSProjectSolution.Data
{
    public class DapperDBConnection
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public DapperDBConnection(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        public IDbConnection CreateConnection()
            => new SqlConnection(_connectionString);
    }
}
