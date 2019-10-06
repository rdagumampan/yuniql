using System;
using System.Data;
using System.Data.SqlClient;

namespace ArdiLabs.Yuniql
{
    public static class DbHelper
    {
        public static void ExecuteNonQuery(SqlConnectionStringBuilder sqlConnectionString, string sqlStatement)
        {
            TraceService.Debug($"Executing sql statement: {Environment.NewLine}{sqlStatement}");

            using (var connection = new SqlConnection(sqlConnectionString.ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = sqlStatement;
                command.CommandTimeout = 0;
                command.ExecuteNonQuery();
            }
        }

        public static int ExecuteScalar(SqlConnectionStringBuilder sqlConnectionString, string sqlStatement)
        {
            TraceService.Debug($"Executing sql statement: {Environment.NewLine}{sqlStatement}");

            var result = 0;
            using (var connection = new SqlConnection(sqlConnectionString.ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = sqlStatement;
                command.CommandTimeout = 0;
                result = command.ExecuteNonQuery();
            }

            return result;
        }

        public static bool QuerySingleBool(SqlConnectionStringBuilder sqlConnectionString, string sqlStatement)
        {
            TraceService.Debug($"Executing sql statement: {Environment.NewLine}{sqlStatement}");

            bool result;
            using (var connection = new SqlConnection(sqlConnectionString.ConnectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = sqlStatement;
                command.CommandTimeout = 0;

                var reader = command.ExecuteReader();
                reader.Read();
                result = Convert.ToBoolean(reader.GetValue(0));
            }

            return result;
        }

        public static string QuerySingleString(SqlConnectionStringBuilder sqlConnectionString, string sqlStatement)
        {
            TraceService.Debug($"Executing sql statement: {Environment.NewLine}{sqlStatement}");

            string result;
            using (var connection = new SqlConnection(sqlConnectionString.ConnectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = sqlStatement;
                command.CommandTimeout = 0;

                var reader = command.ExecuteReader();
                reader.Read();
                result = reader.GetString(0);
            }

            return result;
        }

    }
}
