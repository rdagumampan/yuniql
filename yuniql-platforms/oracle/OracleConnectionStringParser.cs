using Oracle.ManagedDataAccess.Client;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using Yuniql.Extensibility;

namespace Yuniql.Oracle
{
    /// <summary>
    /// 
    /// </summary>
    public class OracleConnectionStringParser
    {
        /// <summary>
        /// 
        /// </summary>
        public OracleConnectionStringParser()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_connectionString"></param>
        /// <returns></returns>
        public ConnectionInfo Parse(string _connectionString)
        {
            //Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=49161))(CONNECT_DATA=(SERVICE_NAME=xe)));User Id=myuser;Password=mypassword;
            var stringParts = _connectionString.Split('(');

            if (_connectionString.Contains("DATA SOURCE", StringComparison.OrdinalIgnoreCase))
            {
                var match = Regex.Match(_connectionString, "DATA SOURCE=(\\w|:|/)*", RegexOptions.IgnoreCase);
                var source = match.Value.Split('=')[1];
                var connectionData = source.Split(':', '/');
                var host = connectionData[0];
                var port = source.Contains(":") ? int.Parse(connectionData[1]) : 1521;
                var serviceName = source.Contains("/") ? connectionData.Last() : string.Empty;
                return new ConnectionInfo { DataSource = $"{host}", Port = port, Database = serviceName };
            }
            else
            {
                //HOST=localhost)
                var hostPair = stringParts.First(s => s.Contains("HOST", StringComparison.InvariantCultureIgnoreCase)).Split("=");
                var host = hostPair[1].Substring(0, hostPair[1].IndexOf(")"));

                //PORT=49161)
                var portPair = stringParts.First(s => s.Contains("PORT", StringComparison.InvariantCultureIgnoreCase)).Split("=");
                var port = Convert.ToInt32(portPair[1].Substring(0, portPair[1].IndexOf(")")).Trim());

                //SERVICE_NAME=xe)
                var serviceNamePair = stringParts.First(s => s.Contains("SERVICE_NAME", StringComparison.InvariantCultureIgnoreCase)).Split("=");
                var serviceName = serviceNamePair[1].Substring(0, serviceNamePair[1].IndexOf(")"));

                var connectionStringBuilder = new OracleConnectionStringBuilder(_connectionString);
                return new ConnectionInfo { DataSource = $"{host}", Port = port, Database = serviceName };
            }

        }
    }
}
