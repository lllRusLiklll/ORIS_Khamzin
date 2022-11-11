using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;

namespace WebServer
{
    public class MyORM
    {
        private readonly IDbConnection _connection = null;
        private IDbCommand _command = null;

        public MyORM(string connectionString)
        {
            _connection = new SqlConnection(connectionString);
            _command = _connection.CreateCommand();
        }

        public T ExecuteScalar<T>(string query, bool isStoredProc = false)
        {
            T result = default!;
            using (_connection)
            {
                if (isStoredProc)
                {
                    _command.CommandType = CommandType.StoredProcedure;
                }
                _command.CommandText = query;
                _connection.Open();
                result = (T)_command.ExecuteScalar()!;
            }
            return result;
        }

        public IEnumerable<T> ExecuteQuery<T>(string query, bool isStoredProc = false)
        {
            var list = new List<T>();
            Type type = typeof(T);
            using (_connection)
            {
                _command.CommandText = query;
                _connection.Open();
                var reader = _command.ExecuteReader();
                while (reader.Read()) 
                {
                    if (isStoredProc)
                    {
                        _command.CommandType = CommandType.StoredProcedure;
                    }
                    T obj = (T)Activator.CreateInstance(type)!;
                    type.GetProperties().ToList().ForEach(p => p.SetValue(obj, reader[p.Name]));
                    list.Add(obj);
                }
            }
            return list;
        }

        public int ExecuteNonQuery(string query, bool isStoredProc = false)
        {
            int noOfAffectedRows = 0;
            using (_connection)
            {
                if (isStoredProc)
                {
                    _command.CommandType = CommandType.StoredProcedure;
                }
                _command.CommandText = query;
                _connection.Open();
                noOfAffectedRows = _command.ExecuteNonQuery();
            }
            return noOfAffectedRows;
        }

        public MyORM AddParameters<T>(string name, T value)
        {
            var param = new SqlParameter(name, value);
            _command.Parameters.Add(param);
            return this;
        }

        public List<T> Select<T>()
        {
            return ExecuteQuery<T>("SELECT * FROM Accounts").ToList();
        }

        public T Select<T>(int id)
        {
            return ExecuteQuery<T>($"SELECT * FROM Accounts WHERE Id={id}").FirstOrDefault()!;
        }

        public void Insert<T>(T item)
        {
            var query = new StringBuilder("INSERT INTO Accounts VALUES (");

            var type = typeof(T);
            type.GetProperties().Skip(1).ToList().ForEach(p => query.Append($"'{p.GetValue(item)}', "));
            query.Remove(query.Length - 2, 2);
            query.Append(')');

            ExecuteNonQuery(query.ToString());
        } 

        public void Update<T>(T item)
        {
            var query = new StringBuilder("UPDATE Accounts SET ");

            var type = typeof(T);
            var props = type.GetProperties();

            props.Skip(1).ToList().ForEach(p => query.Append(p.Name + "=" + $"'{p.GetValue(item)}', "));
            query.Remove(query.Length - 2, 1);
            query.Append("WHERE " + props.First().Name + "=" + props.First().GetValue(item));

            ExecuteNonQuery(query.ToString());
        }

        public void Delete<T>(T item)
        {
            var query = new StringBuilder("DELETE FROM Accounts WHERE ");

            var type = typeof(T);
            var props = type.GetProperties();

            props.Skip(1).ToList().ForEach(p => query.Append(p.Name + "=" + $"'{p.GetValue(item)}' AND "));
            query.Remove(query.Length - 5, 5);

            ExecuteNonQuery(query.ToString());
        }
    }
}
