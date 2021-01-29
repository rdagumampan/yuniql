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
        public DateTime AppliedOnUtc { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// The user id used when migration was performed.
        /// </summary>
        public string AppliedByUser { get; set; }

        /// <summary>
        /// The yuniql client that executed the migration step.
        /// This can be yuniql-cli, yuniql-aspnetcore, yuniql-core, yuniql-azdevops
        /// </summary>
        public string AppliedByTool { get; set; } = "yuniql-cli";

        /// <summary>
        /// The version of client that executed the migration step.
        /// </summary>
        public string AppliedByToolVersion { get; set; } = typeof(DbVersion).Assembly.GetName().Version.ToString();

        /// <summary>
        /// Additional information that describes the execution of the version
        /// </summary>
        public string AdditionalArtifacts { get; set; } = string.Empty;

        /// <summary>
        /// The status of version execution
        /// </summary>
        public Status Status { get; set; } = Status.Successful;

        /// <summary>
        /// The execution time of the version in milliseconds
        /// </summary>
        public int DurationMs { get; set; } = 0;

        /// <summary>
        /// The calculated md5 checksum of the version directory
        /// </summary>
        public string Checksum { get; set; } = string.Empty;

        /// <summary>
        /// The full path of last failed script file
        /// </summary>
        public string FailedScriptPath { get; set; } = string.Empty;

        /// <summary>
        /// The error details from the last failed script file
        /// </summary>
        public string FailedScriptError { get; set; } = string.Empty;
    }
}
