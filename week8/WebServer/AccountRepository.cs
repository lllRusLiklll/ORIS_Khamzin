using WebServer.Controllers;

namespace WebServer
{
    public interface IAccountRepository
    {
        void InsertAccount(Account account);
        void UpdateAccount(Account account);
        void DeleteAccount(Account account);

        List<Account> Query(ISqlSpecification specification);
    }

    public interface ISqlSpecification
    {
        string ToSqlClauses();
    }

    public class AccountRepository : IAccountRepository
    {
        private readonly MyORM _orm;

        public AccountRepository(string connectionString)
        {
            _orm = new MyORM(connectionString);
        }

        public void InsertAccount(Account account)
        {
            _orm.Insert(account);
        }

        public void UpdateAccount(Account account)
        {
            _orm.Update(account);
        }

        public void DeleteAccount(Account account)
        {
            _orm.Delete(account);
        }

        public List<Account> Query(ISqlSpecification specification)
        {
            return _orm.ExecuteQuery<Account>("SELECT * FROM Accounts " + specification.ToSqlClauses()).ToList();
        }
    }

    public class AccountSpecification : ISqlSpecification
    {
        public string ToSqlClauses()
        {
            return "";
        }
    }

    public class AccountSpecificationById : ISqlSpecification
    {
        private readonly int _id;

        public AccountSpecificationById(int id)
        {
            this._id = id;
        }

        public string ToSqlClauses()
        {
            return $"WHERE Id={_id}";
        }
    }
}
