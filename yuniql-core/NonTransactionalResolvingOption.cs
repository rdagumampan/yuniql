using System;
using System.Collections.Generic;
using System.Text;

namespace Yuniql.Core
{
    /// <summary>
    /// Determines the handling behaviour during migration when the previous migration run failed.
    /// </summary>
    public enum NonTransactionalResolvingOption
    {
        /// <summary>
        /// Continue with next script after failure
        /// </summary>
        ContinueAfterFailure
    }
}
