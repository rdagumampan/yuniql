using System;
using System.Data;
using Yuniql.Extensibility;

namespace Yuniql.Core
{
    public static class ConnectionExtensions
    {
        public static IDbCommand CreateCommand(
            this IDbConnection connection,
            string commandText,
            int? commandTimeout = null,
            IDbTransaction transaction = null)
        {
            var command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = commandText;

            if (commandTimeout.HasValue) command.CommandTimeout = commandTimeout.Value;
            if (null != transaction) command.Transaction = transaction;

            return command;
        }

        public static IDbConnection KeepOpen(this IDbConnection connection)
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();

            return connection;
        }

        public static int ExecuteNonQuery(
            this IDbConnection connection,
            string commandText,
            int? commandTimeout = null,
            IDbTransaction transaction = null,
            ITraceService traceService = null)
        {
            if (null != traceService)
                traceService.Debug($"Executing statement: {Environment.NewLine}{commandText}");

            var command = connection
                .KeepOpen()
                .CreateCommand(
                commandText: commandText,
                commandTimeout: commandTimeout,
                transaction: transaction);

            return command.ExecuteNonQuery();
        }

        public static int ExecuteScalar(
            this IDbConnection connection,
            string commandText,
            int? commandTimeout = null,
            IDbTransaction transaction = null,
            ITraceService traceService = null)
        {
            if (null != traceService)
                traceService.Debug($"Executing statement: {Environment.NewLine}{commandText}");

            var command = connection
                .KeepOpen()
                .CreateCommand(
                commandText: commandText,
                commandTimeout: commandTimeout,
                transaction: transaction);

            return command.ExecuteNonQuery();
        }

        public static bool QuerySingleBool(
            this IDbConnection connection,
            string commandText,
            int? commandTimeout = null,
            IDbTransaction transaction = null,
            ITraceService traceService = null)
        {
            if (null != traceService)
                traceService.Debug($"Executing statement: {Environment.NewLine}{commandText}");

            var command = connection
                .KeepOpen()
                .CreateCommand(
                commandText: commandText,
                commandTimeout: commandTimeout,
                transaction: transaction);

            using var reader = command.ExecuteReader();
            if (reader.Read())
                return Convert.ToBoolean(reader.GetValue(0));

            return false;
        }

        public static string QuerySingleString(
            this IDbConnection connection,
            string commandText,
            int? commandTimeout = null,
            IDbTransaction transaction = null,
            ITraceService traceService = null)
        {
            if (null != traceService)
                traceService.Debug($"Executing statement: {Environment.NewLine}{commandText}");

            var command = connection
                .KeepOpen()
                .CreateCommand(
                commandText: commandText,
                commandTimeout: commandTimeout,
                transaction: transaction);

            using var reader = command.ExecuteReader();
            if (reader.Read())
                return reader.GetString(0);

            return null;
        }
    }
}
