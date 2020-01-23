namespace Yuniql.Extensibility
{
    /// <summary>
    /// Extended data to describe the migration version.
    /// </summary>
    public class DbVersionData
    {
        /// <summary>
        /// Unique ID of migration record in migration history table.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The version itself as reflected in the directory structure.
        /// </summary>
        public string Version { get; set; }
        
        /// <summary>
        /// Serialized BLOB of all scripts executed in the version.
        /// </summary>
        public byte[] Artifact { get; set; }
    }
}
