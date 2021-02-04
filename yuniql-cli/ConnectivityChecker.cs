using System;
using System.Net.NetworkInformation;
using Yuniql.Core;
using Yuniql.Extensibility;

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
                using(var connection = _dataService.CreateConnection())
                {
                    connection.Open();
                }
                this._traceService.Success($"Connectivity to database {_connectionInfo.Database} - OK");
                return true;
            }
            catch(Exception e)
            {
                this._traceService.Error($"Connectivity to database {_connectionInfo.Database} failed with error - { e.Message}");
                return false;
            }
        }

        public bool CheckMasterConnectivity()
        {
            try
            {
                using (var connection = _dataService.CreateMasterConnection())
                {
                    connection.Open();
                }
                this._traceService.Success($"Connectivity to master database - OK");
                return true;
            }
            catch (Exception e)
            {
                this._traceService.Error($"Connectivity to master database failed with error - { e.Message}");
                return false;
            }
        }

        public bool CheckServerConnectivity()
        {
            try
            {
                using (Ping ping = new Ping())
                {
                    PingReply pingReply = ping.Send(this._connectionInfo.DataSource);

                    if (pingReply.Status == IPStatus.Success)
                    {
                        _traceService.Success($"Ping check on {this._connectionInfo.DataSource} - OK");
                        return true;
                    }
                    else
                    {
                        _traceService.Error($"Ping check on {this._connectionInfo.DataSource} - FAILED");
                        return false;
                    }
                }
            }
            catch(Exception e)
            {
                _traceService.Error(e.Message);
                return false;
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
