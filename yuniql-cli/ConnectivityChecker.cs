using System;
using System.Net.NetworkInformation;
using Yuniql.Core;
using Yuniql.Extensibility;

//TODO: Rename as ConnectivityService, move to Core, add unit tests, add suggested action to users
namespace Yuniql.CLI
{
    public class ConnectivityChecker : IConnectivityChecker
    {
        private ITraceService _traceService;
        private IDataService _dataService;
        private ConnectionInfo _connectionInfo;

        /// <summary>
        /// Instantiate the ConnectivityChecker using Platform and ConnectionString.
        /// </summary>
        public ConnectivityChecker(string platform, string connectionString, ITraceService traceService)
        {
            if (connectionString == null)
            {
                throw new Exception("No connection string was provided.");
            }

            this._traceService = traceService;
            this._dataService = GetDataService(platform, this._traceService);
            this._dataService.Initialize(connectionString);
            this._connectionInfo = _dataService.GetConnectionInfo();
        }

        /// <summary>
        /// Instantiate the ConnectivityChecker using Initialized Data Service.
        /// </summary>
        public ConnectivityChecker(IDataService dataService, ITraceService traceService)
        {
            this._traceService = traceService;
            this._dataService = dataService;
            this._connectionInfo = _dataService.GetConnectionInfo();
        }

        private IDataService GetDataService(string platform, ITraceService traceService)
        {
            switch (platform)
            {
                case SUPPORTED_DATABASES.SQLSERVER:
                    return new SqlServer.SqlServerDataService(traceService);
                case SUPPORTED_DATABASES.MARIADB:
                    return new MySql.MySqlDataService(traceService);
                case SUPPORTED_DATABASES.MYSQL:
                    return new MySql.MySqlDataService(traceService);
                case SUPPORTED_DATABASES.POSTGRESQL:
                    return new PostgreSql.PostgreSqlDataService(traceService);
                case SUPPORTED_DATABASES.REDSHIFT:
                    return new Redshift.RedshiftDataService(traceService);
                case SUPPORTED_DATABASES.SNOWFLAKE:
                    return new Snowflake.SnowflakeDataService(traceService);
                default:
                    throw new NotSupportedException($"The target database platform {platform} is not supported or plugins location was not correctly configured. " +
                    $"See WIKI for supported database platforms and usage guide.");
            }
        }

        public bool CheckDatabaseConnectivity()
        {
            try
            {
                _traceService.Info($"Verifying sql/odbc connectivity to database {_connectionInfo.Database} on {_connectionInfo.DataSource}...");
                using (var connection = _dataService.CreateConnection())
                {
                    connection.Open();
                }
                _traceService.Success($"Sql/odbc connectivity to database {_connectionInfo.Database} on {_connectionInfo.DataSource} - Successful");
                return true;
            }
            catch (Exception ex)
            {
                _traceService.Error($"Sql/odbc connectivity to database {_connectionInfo.Database} on {_connectionInfo.DataSource} - Failed. Error message: { ex.Message}. " +
                                    $"Suggested action: Check your connection string and verify that the user have sufficient permissions to access the database. " +
                                    $"For sample connection strings, please find your platform at https://www.connectionstrings.com. " +
                                    $"If you think this is a bug, please create an issue ticket here https://github.com/rdagumampan/yuniql/issues.");
                return false;
            }
        }

        public bool CheckMasterConnectivity()
        {
            try
            {
                _traceService.Info($"Verifying sql/odbc connectivity to database master/catalog on {_connectionInfo.DataSource}...");
                using (var connection = _dataService.CreateMasterConnection())
                {
                    connection.Open();
                }
                _traceService.Success($"Sql/odbc connectivity to database master/catalog on {_connectionInfo.DataSource} - Successful");
                return true;
            }
            catch (Exception ex)
            {
                _traceService.Error($"Sql/odbc connectivity to database master/catalog on {_connectionInfo.DataSource} - Failed. Error message: { ex.Message}. " +
                                    $"This maybe an expected behaviour for cloud managed databases such as Azure, AWS and GCP because access to master/catalog databases could be blocked. " +
                                    $"Suggested action: Check your connection string and verify that the user have sufficient permissions to access the database. " +
                                    $"For sample connection strings, please find your platform at https://www.connectionstrings.com. " +
                                    $"If you think this is a bug, please create an issue ticket here https://github.com/rdagumampan/yuniql/issues.");
                return false;
            }
        }


        public bool CheckServerConnectivity()
        {
            try
            {
                using (var ping = new Ping())
                {
                    _traceService.Info($"Verifying tcp/icmp connectivity to database server/cluster {_connectionInfo.DataSource}...");
                    var pingReply = ping.Send(_connectionInfo.DataSource);
                    if (pingReply.Status == IPStatus.Success)
                    {
                        _traceService.Success($"Tcp/icmp connectivity to database server/cluster {_connectionInfo.DataSource} - Successful");
                        return true;
                    }
                    else
                    {
                        WriteTraceError();
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                WriteTraceError(ex);
                return false;
            }

            void WriteTraceError(Exception ex = null)
            {
                var errorMessage = null != ex ? "Error message: " + ex.Message : string.Empty;
                _traceService.Error($"Tcp/icmp connectivity to database server/cluster {_connectionInfo.DataSource} - Failed. {errorMessage} " +
                                    $"This maybe an expected behaviour when the server/cluster is configured to deny remote ping requests. " +
                                    $"If you think this is a bug, please create an issue ticket here https://github.com/rdagumampan/yuniql/issues.");
            }
        }

        public void CheckConnectivity()
        {
            CheckServerConnectivity();
            CheckMasterConnectivity();
            CheckDatabaseConnectivity();
        }
    }
}
