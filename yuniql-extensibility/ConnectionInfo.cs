namespace Yuniql.Extensibility
{
    /// <summary>
    /// Connection information to target database.
    /// </summary>
    public class ConnectionInfo {
 
        /// <summary>
        /// The target database name.
        /// </summary>
        public string Database { get; set; }

        /// <summary>
        /// The host server or database instance in a cluster.
        /// </summary>
        public string DataSource { get; set; }

        /// <summary>
        /// The port number assigned the database access.
        /// </summary>
        public int Port { get; set; }
    }
}
