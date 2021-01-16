using System;

namespace Yuniql.Core
{
    /// <summary>
    /// Custom exception thrown within yuniql migration.
    /// </summary>
    public class YuniqlMigrationException: ApplicationException
    {
        /// <summary>
        /// Custom exception thrown within yuniql migration.
        /// </summary>
        public YuniqlMigrationException(): base()
        {
        }

        /// <summary>
        /// Creates new YuniqlMigrationException.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public YuniqlMigrationException(string message): base(message)
        {
        }

        /// <summary>
        /// Creates new YuniqlMigrationException.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">The exception captured prior to this exception.</param>
        public YuniqlMigrationException(string message, Exception innerException): base(message, innerException)
        {
        }
    }
}