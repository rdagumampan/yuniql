using System;

namespace Yuniql.Extensibility
{
    /// <summary>
    /// Metadata information about migration version.
    /// </summary>
    public class DbVersion
    {
        /// <summary>
        /// Unique sequence id for the version.
        /// </summary>
        public int SequenceId { get; set; }

        /// <summary>
        /// The version itself as reflected in the directory structure.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// The date and time in UTC when migration was run.
        /// </summary>
        public DateTime AppliedOnUtc { get; set; }

        /// <summary>
        /// The user id used when migration was performed.
        /// </summary>
        public string AppliedByUser { get; set; }

        /// <summary>
        /// The version of client that executed the migration step.
        /// This can be yuniql-cli, yuniql-aspnetcore, yuniql-core, yuniql-azdevops
        /// </summary>
        public string AppliedByTool { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string AppliedByToolVersion { get; set; }

        /// <summary>
        /// Gets or sets the status id.
        /// </summary>
        public StatusId StatusId { get; set; } = StatusId.Succeeded;

        /// <summary>
        /// Gets or sets the failed script path.
        /// </summary>
        public string FailedScriptPath { get; set; }

        /// <summary>
        /// Gets or sets the failed script error.
        /// </summary>
        public string FailedScriptError { get; set; }
    }
}
