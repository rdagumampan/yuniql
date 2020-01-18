using System;

namespace Yuniql.Core
{
    public class YuniqlMigrationException: ApplicationException
    {
        public YuniqlMigrationException(): base()
        {
        }
        public YuniqlMigrationException(string message): base(message)
        {
        }

        public YuniqlMigrationException(string message, Exception innerException)
        {
        }
    }
}