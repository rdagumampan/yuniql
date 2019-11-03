using System;
using System.Data;
using System.Data.SqlClient;

namespace ArdiLabs.Yuniql
{
    public static class DbHelper
    {
        public static void ExecuteNonQuery(string connectionString, string sqlStatement)
        {
            TraceService.Info(connectionString);
            TraceService.Debug($"Executing sql statement: {Environment.NewLine}{sqlStatement}");

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = sqlStatement;
                command.CommandTimeout = 0;
                command.ExecuteNonQuery();
            }
        }

        public static bool QuerySingleBool(string connectionString, string sqlStatement)
        {
            TraceService.Info(connectionString);
            TraceService.Debug($"Executing sql statement: {Environment.NewLine}{sqlStatement}");

            bool result = false;
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = sqlStatement;
                command.CommandTimeout = 0;

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        result = Convert.ToBoolean(reader.GetValue(0));
                    }
                }
            }

            return result;
        }

        public static string QuerySingleString(string connectionString, string sqlStatement)
        {
            string result = null;
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = sqlStatement;
                command.CommandTimeout = 0;

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        result = reader.GetString(0);
                    }
                }
            }

            return result;
        }

        public static void ExecuteNonQuery(IDbConnection activeConnection, string sqlStatement, IDbTransaction transaction = null)
        {
            TraceService.Info(activeConnection.ConnectionString);
            TraceService.Debug($"Executing sql statement: {Environment.NewLine}{sqlStatement}");

            var command = activeConnection.CreateCommand();
            command.Transaction = transaction;
            command.CommandType = CommandType.Text;
            command.CommandText = sqlStatement;
            command.CommandTimeout = 0;
            command.ExecuteNonQuery();
        }

        public static int ExecuteScalar(IDbConnection activeConnection, string sqlStatement, IDbTransaction transaction = null)
        {
            TraceService.Info(activeConnection.ConnectionString);
            TraceService.Debug($"Executing sql statement: {Environment.NewLine}{sqlStatement}");

            var result = 0;

            var command = activeConnection.CreateCommand();
            command.Transaction = transaction;
            command.CommandType = CommandType.Text;
            command.CommandText = sqlStatement;
            command.CommandTimeout = 0;
            result = command.ExecuteNonQuery();

            return result;
        }

        public static bool QuerySingleBool(IDbConnection activeConnection, string sqlStatement, IDbTransaction transaction = null)
        {
            TraceService.Info(activeConnection.ConnectionString);
            TraceService.Debug($"Executing sql statement: {Environment.NewLine}{sqlStatement}");

            bool result = false;
            var command = activeConnection.CreateCommand();
            command.Transaction = transaction;
            command.CommandType = CommandType.Text;
            command.CommandText = sqlStatement;
            command.CommandTimeout = 0;

            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    result = Convert.ToBoolean(reader.GetValue(0));
                }
            }

            return result;
        }

        public static string QuerySingleString(IDbConnection activeConnection, string sqlStatement, IDbTransaction transaction = null)
        {
            TraceService.Info(activeConnection.ConnectionString);
            TraceService.Debug($"Executing sql statement: {Environment.NewLine}{sqlStatement}");

            string result = null;

            var command = activeConnection.CreateCommand();
            command.Transaction = transaction;
            command.CommandType = CommandType.Text;
            command.CommandText = sqlStatement;
            command.CommandTimeout = 0;

            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    result = reader.GetString(0);
                }
            }
            return result;
        }

    }
}
