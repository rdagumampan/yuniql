namespace Yuniql.Extensibility
{
    /// <summary>
    /// Extended data to describe the migration version.
    /// </summary>
    public class DbVersionData
    {
        /// <summary>
        /// The version of this artifact schema
        /// </summary>
        public string Version { get; set; }
        
        /// <summary>
        /// Serialized BLOB of all scripts executed in the version.
        /// </summary>
        public byte[] Data { get; set; }
    }
}
