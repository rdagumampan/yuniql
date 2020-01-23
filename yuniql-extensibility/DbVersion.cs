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
        public int Id { get; set; }

        /// <summary>
        /// The version itself as reflected in the directory structure.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// The date and time in UTC when migration was run.
        /// </summary>
        public DateTime DateInsertedUtc { get; set; }

        /// <summary>
        /// The user id used when migration was performed.
        /// </summary>
        public string LastUserId { get; set; }

        /// <summary>
        /// Some additional information on the version.
        /// </summary>
        public string Comment { get; set; }
    }
}
