SETX YUNIQL_TEST_TARGET_PLATFORM "sqlserver"
SETX YUNIQL_TEST_CONNECTION_STRING "Server=localhost,1400;Database=yuniqldb;User Id=SA;Password=P@ssw0rd!"
SETX YUNIQL_TEST_SAMPLEDB "C:\play\yuniql-dev\samples\basic-sqlserver-sample"

SETX YUNIQL_TEST_CLI "C:\play\yuniql-dev\yuniql-cli\bin\release\netcoreapp3.0\win-x64\publish"
SETX YUNIQL_TEST_HOST "LOCAL"

docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=P@ssw0rd!" -p 1400:1433 -d mcr.microsoft.com/mssql/server:2017-latest

@echo off
pause