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
        public Int64 SequenceId { get; set; }

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
        /// The yuniql client that executed the migration step.
        /// This can be yuniql-cli, yuniql-aspnetcore, yuniql-core, yuniql-azdevops
        /// </summary>
        public string AppliedByTool { get; set; }

        /// <summary>
        /// The version of client that executed the migration step.
        /// </summary>
        public string AppliedByToolVersion { get; set; }

        /// <summary>
        /// Additional information that describes the execution of the version
        /// </summary>
        public string AdditionalArtifacts { get; set; }

        /// <summary>
        /// The status of version execution
        /// </summary>
        public Status Status { get; set; } = Status.Successful;

        /// <summary>
        /// The full path of last failed script file
        /// </summary>
        public string FailedScriptPath { get; set; }

        /// <summary>
        /// The error details from the last failed script file
        /// </summary>
        public string FailedScriptError { get; set; }
    }
}
