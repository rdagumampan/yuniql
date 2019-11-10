using ArdiLabs.Yuniql;
using ArdiLabs.Yuniql.Extensibility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Yuniql.Tests
{
    public static class TestDbHelper
    {
        public static List<DbVersion> GetAllDbVersions(string sqlConnectionString)
        {
            var result = new List<DbVersion>();

            var sqlStatement = $"SELECT Id, Version, DateInsertedUtc, LastUserId FROM [dbo].[__YuniqlDbVersion] ORDER BY Version ASC;";

            using (var connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = sqlStatement;
                command.CommandTimeout = 0;

                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var dbVersion = new DbVersion
                    {
                        Id = reader.GetInt16(0),
                        Version = reader.GetString(1),
                        DateInsertedUtc = reader.GetDateTime(2),
                        LastUserId = reader.GetString(3)
                    };
                    result.Add(dbVersion);
                }
            }
            return result;
        }

        public static void ExecuteNonQuery(SqlConnectionStringBuilder sqlConnectionString, string sqlStatement)
        {
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
            string result = null;
            using (var connection = new SqlConnection(sqlConnectionString.ConnectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = sqlStatement;
                command.CommandTimeout = 0;

                var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    result = reader.GetString(0);
                }
            }

            return result;
        }

        public static void ExecuteNonQuery(IDbConnection connection, string sqlStatement)
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = sqlStatement;
            command.CommandTimeout = 0;
            command.ExecuteNonQuery();
        }

        public static int ExecuteScalar(IDbConnection connection, string sqlStatement)
        {
            var result = 0;

            var command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = sqlStatement;
            command.CommandTimeout = 0;
            result = command.ExecuteNonQuery();

            return result;
        }

        public static bool QuerySingleBool(IDbConnection connection, string sqlStatement)
        {
            bool result;
            var command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = sqlStatement;
            command.CommandTimeout = 0;

            var reader = command.ExecuteReader();
            reader.Read();
            result = Convert.ToBoolean(reader.GetValue(0));

            return result;
        }

        public static string QuerySingleString(IDbConnection connection, string sqlStatement)
        {
            string result;

            var command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = sqlStatement;
            command.CommandTimeout = 0;

            var reader = command.ExecuteReader();
            reader.Read();
            result = reader.GetString(0);

            return result;
        }

    }
}
