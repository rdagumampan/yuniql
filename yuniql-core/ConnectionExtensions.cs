using System;
using System.Data;
using System.Diagnostics;
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
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var statementCorrelationId = Guid.NewGuid().ToString().Fixed();
            traceService?.Debug($"Executing statement {statementCorrelationId}: {Environment.NewLine}{commandText}");

            var command = connection
                .KeepOpen()
                .CreateCommand(
                commandText: commandText,
                commandTimeout: commandTimeout,
                transaction: transaction);

            var result = command.ExecuteNonQuery();
            traceService?.Debug($"Affected row(s) {result} for statement {statementCorrelationId}");

            stopwatch.Stop();
            traceService?.Debug($"Statement {statementCorrelationId} executed in {stopwatch.ElapsedMilliseconds} ms");

            return result;
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
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var statementCorrelationId = Guid.NewGuid().ToString().Fixed();
            traceService?.Debug($"Executing statement {statementCorrelationId}: {Environment.NewLine}{commandText}");

            var command = connection
                .KeepOpen()
                .CreateCommand(
                commandText: commandText,
                commandTimeout: commandTimeout,
                transaction: transaction);
            var resultTmp = command.ExecuteScalar();
            var result = DBNull.Value != resultTmp ? Convert.ToInt32(resultTmp) : 0;
            traceService?.Debug($"Affected row(s) {result} for statement {statementCorrelationId}");

            stopwatch.Stop();
            traceService?.Debug($"Statement {statementCorrelationId} executed in {stopwatch.ElapsedMilliseconds} ms");

            return result;
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
            var output = connection.QuerySingle(commandText, commandTimeout: commandTimeout, transaction: transaction, traceService: traceService);
            var result = false;
            if (output != null)
                result = Convert.ToBoolean(output);

            return result;
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
            var output = connection.QuerySingle(commandText, commandTimeout: commandTimeout, transaction: transaction, traceService: traceService);
            string result = null;
            if (output != null)
                result = output as string;

            return result;
        }

        /// <summary>
        /// Executes SQL statement against the active connection and returns scalar value in object.
        /// </summary>
        /// <param name="connection">An active connection.</param>
        /// <param name="commandText">The sql statement to execute with the active connection.</param>
        /// <param name="commandTimeout">Command timeout in seconds.</param>
        /// <param name="transaction">An active transaction.</param>
        /// <param name="traceService">Trace service provider where trace messages will be written.</param>
        /// <returns></returns>
        public static object QuerySingle(
            this IDbConnection connection,
            string commandText,
            int? commandTimeout = null,
            IDbTransaction transaction = null,
            ITraceService traceService = null)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var statementCorrelationId = Guid.NewGuid().ToString().Fixed();
            traceService?.Debug($"Executing statement {statementCorrelationId}: {Environment.NewLine}{commandText}");

            var command = connection
                .KeepOpen()
                .CreateCommand(
                commandText: commandText,
                commandTimeout: commandTimeout,
                transaction: transaction);

            object result = null;
            using var reader = command.ExecuteReader();
            if (reader.Read())
                result = reader.GetValue(0);

            stopwatch.Stop();
            traceService?.Debug($"Statement {statementCorrelationId} executed in {stopwatch.ElapsedMilliseconds} ms");

            return result;
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
            var output = connection.QuerySingle(commandText, commandTimeout: commandTimeout, transaction: transaction, traceService: traceService);
            var result = false;
            if (output != null)
                result = true;

            return result;
        }
    }
}
