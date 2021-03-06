﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Data;

namespace CSharp.Util.Data
{
    public sealed class DbHelper
    {
        public string ConnectionStr { get; set; }

        private static DbProviderFactory providerFactory;

        #region 构造函数
        public DbHelper(string connectionString, DbProviderFactory provider)
        {
            this.ConnectionStr = connectionString;
            providerFactory = provider;
        }
        public DbHelper(string connecntionString, DbProviderType providerType)
        {
            this.ConnectionStr = connecntionString;
            providerFactory = ProviderFactory.GetDbProviderFactory(providerType);
            if (providerFactory == null)
            {
                throw new ArgumentException("provider exception");
            }

        }
        #endregion

        #region ExecuteNonQuery
        public int ExecuteNonQuery(string commandText, IList<DbParameter> parameters)
        {
            return ExecuteNonQuery(commandText, parameters, CommandType.Text);
        }

        public int ExecuteNonQuery(string commandText, IList<DbParameter> parameters, CommandType commandType)
        {
            using (DbCommand command = CreateCommand(commandText, parameters, commandType))
            {
                command.Connection.Open();
                int affectedRows = command.ExecuteNonQuery();
                command.Connection.Close();
                return affectedRows;
            }
        }
        #endregion

        #region ExecuteReader
        public DbDataReader ExecuteReader(string commandText, IList<DbParameter> parameters)
        {
            return ExecuteReader(commandText, parameters, CommandType.Text);
        }

        public DbDataReader ExecuteReader(string commandText, IList<DbParameter> parameters, CommandType commandType)
        {
            DbCommand command = CreateCommand(commandText, parameters, commandType);
            command.Connection.Open();
            return command.ExecuteReader(CommandBehavior.CloseConnection);
        }
        #endregion

        #region ExecuteScalar
        public T ExecuteScalar<T>(string commandText, IList<DbParameter> parameters) where T : new()
        {
            return ExecuteScalar<T>(commandText, parameters, CommandType.Text);
        }

        public T ExecuteScalar<T>(string commandText, IList<DbParameter> parameters, CommandType commandType)
        where T : new()
        {
            object result = ExecuteScalar(commandText, parameters, commandType);
            if (result != null)
            {
                return (T)Convert.ChangeType(result, typeof(T)); ;
            }
            return default(T);
        }

        public object ExecuteScalar(string commandText, IList<DbParameter> parameters)
        {
            return ExecuteScalar(commandText, parameters);
        }

        public object ExecuteScalar(string commandText, IList<DbParameter> parameters, CommandType commandType)
        {
            using (DbCommand command = CreateCommand(commandText, parameters, commandType))
            {
                command.Connection.Open();
                object result = command.ExecuteScalar();
                command.Connection.Close();
                return result;
            }
        }
        #endregion

        #region ExecuteDataRow

        public T ExecuteDataRow<T>(string commandText, IList<DbParameter> parameters) where T : new()
        {
            return ExecuteDataRow<T>(commandText, parameters, CommandType.Text);
        }

        public T ExecuteDataRow<T>(string commandText, IList<DbParameter> parameters, CommandType commandType) where T : new()
        {
            DataRow row = ExecuteDataRow(commandText, parameters, commandType);
            return DataTableHelper.Map<T>(row);
        }

        public DataRow ExecuteDataRow(string commandText, IList<DbParameter> parameters)
        {
            return ExecuteDataRow(commandText, parameters, CommandType.Text);
        }

        public DataRow ExecuteDataRow(string commandText, IList<DbParameter> parameters, CommandType commandType)
        {
            using (DbCommand command = CreateCommand(commandText, parameters, commandType))
            {
                DataTable dt = ExecuteDataTable(commandText, parameters, commandType);
                return dt.Rows.Count > 0 ? dt.Rows[0] : null;
            }
        }
        #endregion

        #region ExecuteDataTable
        public List<T> ExecuteDataTable<T>(string commandText, IList<DbParameter> parameters) where T : new()
        {
            return ExecuteDataTable<T>(commandText, parameters, CommandType.Text);
        }

        public List<T> ExecuteDataTable<T>(string commandText, IList<DbParameter> parameters, CommandType commandType) where T : new()
        {
            var dt = ExecuteDataTable(commandText, parameters, commandType);
            return DataTableHelper.Map<T>(dt);
        }

        public DataTable ExecuteDataTable(string commandText, IList<DbParameter> parameters)
        {
            return ExecuteDataTable(commandText, parameters, CommandType.Text);
        }

        public DataTable ExecuteDataTable(string commandText, IList<DbParameter> parameters, CommandType commantType)
        {
            using (DbCommand command = CreateCommand(commandText, parameters, commantType))
            {
                using (DbDataAdapter adapter = providerFactory.CreateDataAdapter())
                {
                    adapter.SelectCommand = command;
                    DataTable data = new DataTable();
                    adapter.Fill(data);
                    return data;
                }
            }
        }
        #endregion

        #region DataSet
        public DataSet ExecuteDataSet(string commandText, IList<DbParameter> parameters)
        {
            return ExecuteDataSet(commandText, parameters, CommandType.Text);
        }
        public DataSet ExecuteDataSet(string commandText, IList<DbParameter> parameters, CommandType commandType)
        {
            using (DbCommand command = CreateCommand(commandText, parameters, commandType))
            {
                using (DbDataAdapter adapter = providerFactory.CreateDataAdapter())
                {
                    DataSet ds = new DataSet();
                    adapter.Fill(ds);
                    return ds;
                }
            }
        }
        #endregion

        #region Entity
        public List<T> QueryForList<T>(string commandText, IList<DbParameter> parameters) where T : new()
        {
            return QueryForList<T>(commandText, parameters, CommandType.Text);
        }
        public List<T> QueryForList<T>(string commandText, IList<DbParameter> parameters, CommandType commandType) where T : new()
        {
            //TODO
            DataTable data = ExecuteDataTable(commandText, parameters, commandType);
            return null;
        }
        #endregion

        #region DbParameter
        public DbParameter CreateDbParameter(string name, object value)
        {
            return CreateParameter(name, ParameterDirection.Input, value);
        }

        public DbParameter CreateParameter(string name, ParameterDirection parameterDirection, object value)
        {
            DbParameter parameter = providerFactory.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value;
            parameter.Direction = parameterDirection;
            return parameter;
        }
        #endregion

        private DbCommand CreateCommand(string commandText, IList<DbParameter> parameters, CommandType commandType)
        {
            DbConnection connection = providerFactory.CreateConnection();
            DbCommand command = providerFactory.CreateCommand();
            connection.ConnectionString = ConnectionStr;
            command.CommandText = commandText;
            command.CommandType = commandType;
            command.Connection = connection;
            command.Transaction = null;
            if (!(parameters == null) || parameters.Count == 0)
            {
                foreach (DbParameter parameter in parameters)
                    command.Parameters.Add(parameter);
            }
            return command;
        }

    }
    public class ProviderFactory
    {
        private static Dictionary<DbProviderType, string> providerInvariantNames = new Dictionary<DbProviderType, string>();
        private static Dictionary<DbProviderType, DbProviderFactory> providerFactoies = new Dictionary<DbProviderType, DbProviderFactory>(20);

        static ProviderFactory()
        {
            //加载已知的数据库访问类的程序集  
            providerInvariantNames.Add(DbProviderType.SqlServer, "System.Data.SqlClient");
            providerInvariantNames.Add(DbProviderType.OleDb, "System.Data.OleDb");
            providerInvariantNames.Add(DbProviderType.ODBC, "System.Data.ODBC");
            providerInvariantNames.Add(DbProviderType.Oracle, "Oracle.DataAccess.Client");
            providerInvariantNames.Add(DbProviderType.MySql, "MySql.Data.MySqlClient");
            providerInvariantNames.Add(DbProviderType.SQLite, "System.Data.SQLite");
            providerInvariantNames.Add(DbProviderType.Firebird, "FirebirdSql.Data.Firebird");
            providerInvariantNames.Add(DbProviderType.PostgreSql, "Npgsql");
            providerInvariantNames.Add(DbProviderType.DB2, "IBM.Data.DB2.iSeries");
            providerInvariantNames.Add(DbProviderType.Informix, "IBM.Data.Informix");
            providerInvariantNames.Add(DbProviderType.SqlServerCe, "System.Data.SqlServerCe");
        }

        //public static string GetProviderInvariantName(DbProviderType providerType)
        //{

        //}

        public static DbProviderFactory GetDbProviderFactory(DbProviderType providerType)
        {
            if (!providerFactoies.ContainsKey(providerType))
            {
                //TODO 加载未知provider
                providerFactoies.Add(providerType, ImportDbProviderFactory(providerType));
            }
            return providerFactoies[providerType];
        }

        private static DbProviderFactory ImportDbProviderFactory(DbProviderType providerType)
        {
            string providerName = providerInvariantNames[providerType];
            DbProviderFactory factory = null;
            try
            {
                factory = DbProviderFactories.GetFactory(providerName);
            }
            catch (ArgumentException e)
            {
                factory = null;
            }
            return factory;
        }
    }

    public enum DbProviderType : byte
    {
        SqlServer,
        MySql,
        Oracle,
        ODBC,
        OleDb,
        SQLite,
        Firebird,
        PostgreSql,
        DB2,
        Informix,
        SqlServerCe
    }
}