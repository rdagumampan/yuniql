using System;
using System.Collections.Generic;
using System.Text;

namespace Yuniql.Core
{
    public enum NonTransactionalResolvingOption
    {
        /// <summary>
        /// Continue with next script after failure
        /// </summary>
        ContinueAfterFailure
    }
}
