using System;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Yuniql.Extensibility;

namespace Yuniql.Core
{
    /// <summary>
    /// Check and test connectivity to target database server.
    /// </summary>
    public class ConnectivityService : IConnectivityService
    {
        private ITraceService _traceService;
        private IDataService _dataService;
        private ConnectionInfo _connectionInfo;

        /// <summary>
        /// Instantiate the ConnectivityChecker using Platform and ConnectionString.
        /// </summary>
        public ConnectivityService(IDataService dataService, ITraceService traceService)
        {
            this._traceService = traceService;
            this._dataService = dataService;
            this._connectionInfo = _dataService.GetConnectionInfo();
        }

        /// <summary>
        /// Checks if we can establish sql/odbc connectivity to database on target server/cluster
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Checks if we can establish sql/odbc connectivity to database master/catalog on target server/cluster
        /// </summary>
        /// <returns></returns>
        public bool CheckMasterConnectivity()
        {
            try
            {
                _traceService.Info($"Verifying sql/odbc connectivity to master/catalog on {_connectionInfo.DataSource}...");
                using (var connection = _dataService.CreateMasterConnection())
                {
                    connection.Open();
                }
                _traceService.Success($"Sql/odbc connectivity to master/catalog on {_connectionInfo.DataSource} - Successful");
                return true;
            }
            catch (Exception ex)
            {
                _traceService.Error($"Sql/odbc connectivity to master/catalog on {_connectionInfo.DataSource} - Failed. Error message: { ex.Message}. " +
                                    $"This maybe an expected behaviour for cloud managed databases such as Azure, AWS and GCP because access to master/catalog databases could be blocked. " +
                                    $"Suggested action: Check your connection string and verify that the user have sufficient permissions to access the database. " +
                                    $"For sample connection strings, please find your platform at https://www.connectionstrings.com. " +
                                    $"If you think this is a bug, please create an issue ticket here https://github.com/rdagumampan/yuniql/issues.");
                return false;
            }
        }

        /// <summary>
        /// Checks if the service can be pinged
        /// </summary>
        /// <returns></returns>
        public bool CheckServerConnectivity()
        {
            try
            {
                using (var ping = new Ping())
                {
                    _traceService.Info($"Verifying ping connectivity to server/cluster {_connectionInfo.DataSource}...");
                    var pingReply = ping.Send(_connectionInfo.DataSource);
                    if (pingReply.Status == IPStatus.Success)
                    {
                        _traceService.Success($"Ping connectivity to server/cluster {_connectionInfo.DataSource} - Successful");
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
                _traceService.Error($"Ping connectivty to server/cluster {_connectionInfo.DataSource} - Failed. {errorMessage} " +
                                    $"This maybe an expected behaviour when the server/cluster is configured to deny remote ping requests. " +
                                    $"If you think this is a bug, please create an issue ticket here https://github.com/rdagumampan/yuniql/issues.");
            }
        }

        /// <summary>
        /// Checks if the target port is open
        /// </summary>
        /// <returns></returns>
        public bool CheckServerPortConnectivity()
        {
            try
            {
                _traceService.Info($"Verifying port opening to server/cluster on {_connectionInfo.DataSource}...");
                var timeout = TimeSpan.FromSeconds(5);
                using (var client = new TcpClient())
                {
                    var result = client.BeginConnect(_connectionInfo.DataSource, _connectionInfo.Port, null, null);
                    var success = result.AsyncWaitHandle.WaitOne(timeout);
                    client.EndConnect(result);
                    _traceService.Success($"Port opening verification to server/cluster {_connectionInfo.DataSource} - Successful");

                    return success;
                }
            }
            catch (Exception ex)
            {
                _traceService.Error($"Port opeining  verification to server/cluster {_connectionInfo.DataSource} - Failed. {ex.Message} " +
                                    $"This maybe an expected behaviour when the server/cluster is configured to deny remote ping requests. " +
                                    $"If you think this is a bug, please create an issue ticket here https://github.com/rdagumampan/yuniql/issues.");
                return false;
            }
        }

        /// <summary>
        /// Check for connectivity to target server/cluster and database
        /// </summary>
        public void CheckConnectivity()
        {
            CheckServerConnectivity();

            if (_connectionInfo.Port > 0)
                CheckServerPortConnectivity();

            CheckMasterConnectivity();
            CheckDatabaseConnectivity();
        }
    }
}
