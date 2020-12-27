using System;
using System.Collections.Generic;
using System.Text;

namespace Yuniql.Core
{
    /// <summary>
    /// Determines the handling behaviour during migration when the previous migration run failed.
    /// This option is only applicable for platforms not supporting transaction DDL.
    /// </summary>
    public enum NonTransactionalResolvingOption
    {
        /// <summary>
        /// Continue with version after the last failed version. 
        /// This assumes the last failed version was handled and fixed manually.
        /// </summary>
        ContinueAfterFailure
    }
}
