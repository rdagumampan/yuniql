using Yuniql.Extensibility;

namespace Yuniql.Core
{
    public class NonTransactionalContext
    {
        public NonTransactionalContext(DbVersion failedDbVersion, bool continueAfterFailure)
        {
            this.FailedScriptPath = failedDbVersion.FailedScriptPath;
            this.ContinueAfterFailure = continueAfterFailure;
        }

        /// <summary>
        /// Gets the failed script path.
        /// </summary>
        public string FailedScriptPath { get; }

        /// <summary>
        /// Gets the resolution option.
        /// </summary>
        public bool? ContinueAfterFailure { get; }

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
