using WebServer.Controllers;

namespace WebServer
{
    public interface IAccountDAO
    {
        List<Account> GetAccounts();
        Account GetAccountById(int id);
        void Insert(Account account);
        void Update(Account account);
        void Delete(Account account);
    }

    public class AccountDAO : IAccountDAO
    {
        private readonly MyORM _orm;
        
        public AccountDAO(string connectionString)
        {
            _orm = new MyORM(connectionString);
        }

        public List<Account> GetAccounts()
        {
            return _orm.Select<Account>();
        }

        public Account GetAccountById(int id)
        {
            return _orm.Select<Account>(id);
        }

        public void Insert(Account account)
        {
            _orm.Insert(account);
        }

        public void Update(Account account)
        {
            _orm.Update(account);
        }

        public void Delete(Account account)
        {
            _orm.Delete(account);
        }
    }
}
