using System;
using System.Data;
using Yuniql.Extensibility;

namespace Yuniql.Core
{
    /// <summary>
    /// 
    /// </summary>
    public static class ConnectionExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="commandText"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        public static IDbConnection KeepOpen(this IDbConnection connection)
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();

            return connection;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="commandText"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="transaction"></param>
        /// <param name="traceService"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="commandText"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="transaction"></param>
        /// <param name="traceService"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="commandText"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="transaction"></param>
        /// <param name="traceService"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="commandText"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="transaction"></param>
        /// <param name="traceService"></param>
        /// <returns></returns>
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
