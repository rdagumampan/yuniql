using System;
using System.Collections.Generic;
using System.Text;

namespace Yuniql.Extensibility
{
    /// <summary>
    /// Support for non-transactional flow (when version scripts are not executed in transaction)
    /// </summary>
    public interface INonTransactionalFlow
    {
        /// <summary>
        /// Gets the SQL for insert or update version for non-transactional platform.
        /// </summary>
        /// <returns></returns>
        public string GetSqlForUpsertVersion();
    }
}
