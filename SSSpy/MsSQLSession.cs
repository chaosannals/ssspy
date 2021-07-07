using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using SSSpy.Exceptions;

namespace SSSpy
{
    public class MsSQLSession : IDisposable
    {
        public string ConnectString { get; private set; }
        public SqlConnection Connection { get; private set; }
        public SqlTransaction Transaction { get; private set; }
        public bool TransactionValid { get; private set; }

        /// <summary>
        /// 初始化。
        /// </summary>
        /// <param name="server"></param>
        /// <param name="database"></param>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <param name="trusted"></param>
        public MsSQLSession(string server, string database, string user, string password, bool trusted)
        {
            ConnectString = string.Format("Server={0};Database={1};User Id={2};Password={3};Trusted_Connection={4}", server, database, user, password, trusted.ToString());
            Connection = null;
            Transaction = null;
            TransactionValid = false;
        }

        public MsSQLSession(string server, string user, string password, bool isSspi = true)
        {
            string dns = string.Format("Server={0}; User Id={1};Password={2}", server, user, password);
            if (isSspi) dns += ";Trusted_Connection=SSPI";
            ConnectString = dns;
            Connection = null;
            Transaction = null;
            TransactionValid = false;
        }

        /// <summary>
        /// 开始事务。
        /// </summary>
        public void StartTransaction()
        {
            EnsureConnection();
            Transaction = Connection.BeginTransaction();
        }

        /// <summary>
        /// 提交事务。
        /// </summary>
        public void CommitTransaction()
        {
            Transaction.Commit();
            TransactionValid = true;
        }

        /// <summary>
        /// 确保连接。
        /// </summary>
        public void EnsureConnection()
        {
            if (Connection == null)
            {
                Connection = new SqlConnection(ConnectString);
            }
            switch (Connection.State)
            {
                case ConnectionState.Broken:
                    Connection.Close();
                    Connection.Open();
                    break;
                case ConnectionState.Closed:
                    Connection.Open();
                    break;
            }
        }

        /// <summary>
        /// 获取数据列表。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public List<T> Search<T>(string sql, params SqlParameter[] args)
        {
            using (SqlCommand command = NewCommand(sql, args))
            {
                SqlDataReader reader = command.ExecuteReader();
                List<T> result = new List<T>();
                Type type = typeof(T);
                PropertyInfo[] properties = type.GetProperties();
                while (reader.Read())
                {
                    T one = (T)type.Assembly.CreateInstance(type.FullName);
                    foreach (PropertyInfo p in properties)
                    {
                        try
                        {
                            var v = reader[p.Name];
                            p.SetValue(one, v, null);
                        }
                        catch (IndexOutOfRangeException){}
                    }
                    result.Add(one);
                }
                reader.Close();
                return result;
            }
        }

        public T Find<T>(string sql, params SqlParameter[] args) where T : class
        {
            using (SqlCommand command = NewCommand(sql, args))
            {
                SqlDataReader reader = command.ExecuteReader();
                try
                {
                    if (!reader.Read())
                    {
                        return null;
                    }
                    Type type = typeof(T);
                    T result = type.Assembly.CreateInstance(type.FullName) as T;
                    for (int i = 0; i < reader.FieldCount; ++i)
                    {
                        string pn = reader.GetName(i);
                        PropertyInfo p = type.GetProperty(pn);
                        if (p != null)
                        {
                            object v = reader[pn];
                            p.SetValue(result, v, null);
                        }
                    }
                    return result;
                }
                finally
                {
                    reader.Close();
                }
            }
        }

        public T Pick<T>(string sql, params SqlParameter[] args)
        {
            using (SqlCommand command = NewCommand(sql, args))
            {
                SqlDataReader reader = command.ExecuteReader();
                try
                {
                    T result = reader.Read() ? (T)reader[0] : default(T);
                    return result;
                }
                finally
                {
                    reader.Close();
                }
            }
        }

        public object Pick(string sql, params SqlParameter[] args)
        {
            using (SqlCommand command = NewCommand(sql, args))
            {
                SqlDataReader reader = command.ExecuteReader();
                try
                {
                    return reader.Read() ? reader[0] : null;
                }
                finally
                {
                    reader.Close();
                }
            }
        }

        /// <summary>
        /// 判断是否存在。
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public bool Has(string sql, params SqlParameter[] args)
        {
            using (SqlCommand command = NewCommand(sql, args))
            {
                SqlDataReader reader = command.ExecuteReader();
                bool result = reader.HasRows;
                reader.Close();
                return result;
            }
        }

        /// <summary>
        /// 计算行数。
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public long Count(string sql, params SqlParameter[] args)
        {
            using (SqlCommand command = NewCommand(sql, args))
            {
                return long.Parse(command.ExecuteScalar().ToString());
            }
        }

        /// <summary>
        /// 创建一个新命令。
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public SqlCommand NewCommand(string sql, params SqlParameter[] args)
        {
            EnsureConnection();
            SqlCommand command = new SqlCommand();
            command.Connection = Connection;
            command.CommandType = CommandType.Text;
            command.CommandText = sql;
            if (Transaction != null)
            {
                command.Transaction = Transaction;
            }
            if (args.Length > 0)
            {
                command.Parameters.AddRange(args);
            }
            return command;
        }

        /// <summary>
        /// 添加一个数据。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="one"></param>
        /// <returns></returns>
        public int Add<T>(T one, string table = null)
        {
            Type type = typeof(T);
            PropertyInfo[] properties = type.GetProperties();
            StringBuilder builder = new StringBuilder();
            builder.Append("INSERT INTO [");
            builder.Append(table ?? type.Name);
            builder.Append("](");
            List<string> fields = new List<string>();
            List<string> vhs = new List<string>();
            List<SqlParameter> vs = new List<SqlParameter>();
            foreach (PropertyInfo p in properties)
            {
                object v = p.GetValue(one, null);
                fields.Add(string.Format("[{0}]", p.Name));
                string h = string.Format("@{0}", p.Name);
                vhs.Add(h);
                vs.Add(new SqlParameter(h, v));
            }
            builder.Append(string.Join(",", fields.ToArray()));
            builder.Append(")VALUES(");
            builder.Append(string.Join(",", vhs.ToArray()));
            builder.Append(")");
            // builder.ToString().Log();
            return Execute(builder.ToString(), vs.ToArray());
        }

        /// <summary>
        /// 添加并获取 ID
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="one"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        public int AddGetId<T>(T one, string table = null)
        {
            Add(one);
            string rsql = string.Format("SELECT IDENT_CURRENT('{0}')", table ?? typeof(T).Name);
            // rsql.Log();
            using (SqlCommand command = NewCommand(rsql))
            {
                var reader = command.ExecuteReader();
                try
                {
                    reader.Read();
                    return int.Parse(reader[0].ToString());
                }
                finally
                {
                    reader.Close();
                }
            }
        }

        /// <summary>
        /// 通过 ID 更新数据。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="one"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        public int Edit<T>(T one, string tag = "ID", string table = null)
        {
            Type type = typeof(T);
            PropertyInfo[] properties = type.GetProperties();
            StringBuilder builder = new StringBuilder();
            builder.Append("UPDATE [");
            builder.Append(table ?? type.Name);
            builder.Append("] SET ");
            List<string> sets = new List<string>();
            List<SqlParameter> vs = new List<SqlParameter>();
            foreach (PropertyInfo p in properties)
            {
                if (p.Name == tag) continue;
                var v = p.GetValue(one, null);
                string h = string.Format("@{0}", p.Name);
                vs.Add(new SqlParameter(h, v));
                sets.Add(string.Format("[{0}]=@{1}", p.Name, p.Name));
            }
            builder.Append(string.Join(",", sets.ToArray()));

            var tagProperty = type.GetProperty(tag);
            if (tagProperty == null)
            {
                throw new InvalidSQLException("标记字段无效");
            }

            var tagValue = tagProperty.GetValue(one, null);
            if (tagValue == null)
            {
                throw new InvalidSQLException("标记数据不可空");
            }

            builder.Append(string.Format(" WHERE [{0}]={1}", tag, tagValue));
            // builder.ToString().Log();
            return Execute(builder.ToString(), vs.ToArray());
        }

        /// <summary>
        /// 执行语句
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public int Execute(string sql, params SqlParameter[] args)
        {
            using (SqlCommand command = NewCommand(sql, args))
            {
                return command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// 回收，事务没有提交就回滚，连接断开。
        /// </summary>
        public void Dispose()
        {
            if (Transaction != null && !TransactionValid)
            {
                Transaction.Rollback();
            }
            if (Connection != null && Connection.State != ConnectionState.Closed)
            {
                Connection.Dispose();
            }
        }
    }
}
