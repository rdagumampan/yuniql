using System;
using System.Data;
using Yuniql.Extensibility;

namespace Yuniql.Core
{
    /// <summary>
    /// Connection extensions for most common data access methods
    /// </summary>
    public static class ConnectionExtensions
    {
        /// <summary>
        /// Creates intance of <see cref="IDbCommand" using the active connection object./>
        /// </summary>
        /// <param name="connection">An active connection.</param>
        /// <param name="commandText">The sql statement to execute with the active connection.</param>
        /// <param name="commandTimeout">Command timeout in seconds.</param>
        /// <param name="transaction">An active transaction.</param>
        /// <returns>An instance of command.</returns>
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
        /// Opens the connection when its found to be closed.
        /// </summary>
        /// <param name="connection">A connection.</param>
        /// <returns>An active connection.</returns>
        public static IDbConnection KeepOpen(this IDbConnection connection)
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();

            return connection;
        }

        /// <summary>
        /// Execute SQL statement against the active connection and returns number of affected rows.
        /// </summary>
        /// <param name="connection">An active connection.</param>
        /// <param name="commandText">The sql statement to execute with the active connection.</param>
        /// <param name="commandTimeout">Command timeout in seconds.</param>
        /// <param name="transaction">An active transaction.</param>
        /// <param name="traceService">Trace service provider where trace messages will be written.</param>
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
        /// Executes SQL statement against the active connection and returns single row single column result.
        /// </summary>
        /// <param name="connection">An active connection.</param>
        /// <param name="commandText">The sql statement to execute with the active connection.</param>
        /// <param name="commandTimeout">Command timeout in seconds.</param>
        /// <param name="transaction">An active transaction.</param>
        /// <param name="traceService">Trace service provider where trace messages will be written.</param>
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
            var result = command.ExecuteScalar();

            return DBNull.Value != result ? Convert.ToInt32(result) : 0;
        }

        /// <summary>
        /// Executes SQL statement against the active connection and returns scalar value in boolean.
        /// </summary>
        /// <param name="connection">An active connection.</param>
        /// <param name="commandText">The sql statement to execute with the active connection.</param>
        /// <param name="commandTimeout">Command timeout in seconds.</param>
        /// <param name="transaction">An active transaction.</param>
        /// <param name="traceService">Trace service provider where trace messages will be written.</param>
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
        /// Executes SQL statement against the active connection and returns scalar value in string.
        /// </summary>
        /// <param name="connection">An active connection.</param>
        /// <param name="commandText">The sql statement to execute with the active connection.</param>
        /// <param name="commandTimeout">Command timeout in seconds.</param>
        /// <param name="transaction">An active transaction.</param>
        /// <param name="traceService">Trace service provider where trace messages will be written.</param>
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

        /// <summary>
        /// Executes SQL statement against the active connection and returns scalar value in string.
        /// </summary>
        /// <param name="connection">An active connection.</param>
        /// <param name="commandText">The sql statement to execute with the active connection.</param>
        /// <param name="commandTimeout">Command timeout in seconds.</param>
        /// <param name="transaction">An active transaction.</param>
        /// <param name="traceService">Trace service provider where trace messages will be written.</param>
        /// <returns></returns>
        public static bool QuerySingleRow(
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
                return true;

            return false;
        }
    }
}
