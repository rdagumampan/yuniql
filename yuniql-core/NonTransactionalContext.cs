using System;
using System.Collections.Generic;
using System.Text;
using Yuniql.Extensibility;

namespace Yuniql.Core
{
    public class NonTransactionalContext
    {
        public NonTransactionalContext(DbVersion failedDbVersion, NonTransactionalResolvingOption resolvingOption)
        {
            this.FailedScriptPath = failedDbVersion.FailedScriptPath;
            this.ResolvingOption = resolvingOption;
        }

        /// <summary>
        /// Gets the failed script path.
        /// </summary>
        public string FailedScriptPath { get; }

        /// <summary>
        /// Gets the resolution option.
        /// </summary>
        public NonTransactionalResolvingOption ResolvingOption { get; }

        /// <summary>
        /// Gets a value indicating whether failed script path is matched.
        /// </summary>
        public bool IsFailedScriptPathMatched { get; private set; } = false;

        /// <summary>
        /// Sets the failed script path as matched.
        /// </summary>
        /// <returns></returns>
        public void SetFailedScriptPathMatch()
        {
            this.IsFailedScriptPathMatched = true;
        }
    }
}
