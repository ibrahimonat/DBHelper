using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data;
using System.Text.RegularExpressions;

namespace DBHelper
{
    public static class Db
    {
        static string connectionString;

        [ThreadStatic]
        static SqlConnection connection;
        [ThreadStatic]
        static SqlTransaction transaction;

        static Db()
        {
            connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        }

        public static bool OpenConnection()
        {
            if (connection == null)
            {
                connection = new SqlConnection(connectionString);
            }
            if (connection.State == ConnectionState.Closed ||
                connection.State == ConnectionState.Broken)
            {
                connection.Open();
                return true;
            }
            return false;
        }

        static void CloseConnection(bool close)
        {
            if (close)
            {
                connection.Close();
                connection.Dispose();
                connection = null;
            }
        }

        static void SetParameterToCommand(SqlCommand command, string sql, params object[] parameterValues)
        {
            string pattern = @"@\w+";
            var parameterNamesFromQuery = Regex.Matches(sql, pattern);
            if (parameterNamesFromQuery.Count != parameterValues.Length)
                throw new Exception("Parametre eksik veya fazla!");

            for (int i = 0; i < parameterNamesFromQuery.Count; i++)
            {
                command.Parameters.AddWithValue(parameterNamesFromQuery[i].Value, parameterValues[i] ?? DBNull.Value);
            }
        }

        public static bool BeginTransaction()
        {
            if (transaction == null)
            {
                transaction = connection.BeginTransaction();
                return true;
            }
            return false;
        }

        public static void CommitTransaction(bool commit)
        {
            if (commit)
            {
                transaction.Commit();
                transaction.Dispose();
                transaction = null;
            }
        }

        public static void RollbackTransaction()
        {
            if (transaction != null)
            {
                transaction.Rollback();
                transaction.Dispose();
                transaction = null;
            }
        }

        public static object ExecuteScalar(string sql, params object[] parameterValues)
        {
            bool con = OpenConnection();
            using (SqlCommand cmd = new SqlCommand(sql, connection))
            {
                SetParameterToCommand(cmd, sql, parameterValues);

                if (transaction != null)
                    cmd.Transaction = transaction;

                try
                {
                    return cmd.ExecuteScalar();
                }
                finally
                {
                    CloseConnection(con);
                }
            }
        }

        public static int ExecuteNonQuery(string sql, params object[] parameterValues)
        {
            bool con = OpenConnection();
            using (SqlCommand cmd = new SqlCommand(sql, connection))
            {
                SetParameterToCommand(cmd, sql, parameterValues);

                if (transaction != null)
                    cmd.Transaction = transaction;

                try
                {
                    return cmd.ExecuteNonQuery();
                }
                finally
                {
                    CloseConnection(con);
                }
            }
        }

        public static IDataReader ExecuteReader(string sql, params object[] parameterValues)
        {
            bool con = OpenConnection();
            SqlCommand cmd = new SqlCommand(sql, connection);
            SetParameterToCommand(cmd, sql, parameterValues);
            return cmd.ExecuteReader(CommandBehavior.CloseConnection);
        }

        public static void FillTable(DataTable dataTable, string sql, params object[] parameterValues)
        {
            bool con = OpenConnection();
            using (SqlDataAdapter dap = new SqlDataAdapter(sql, connection))
            {
                dap.SelectCommand.CommandTimeout = 100;
                SetParameterToCommand(dap.SelectCommand, sql, parameterValues);

                if (transaction != null)
                    dap.SelectCommand.Transaction = transaction;

                try
                {
                    dap.Fill(dataTable);
                }
                finally
                {
                    CloseConnection(con);
                }
            }
        }

    }
}
