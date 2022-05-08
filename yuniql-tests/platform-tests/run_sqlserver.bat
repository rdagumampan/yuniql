@echo on

echo "-------- Preparing environment variables"
SETX YUNIQL_TEST_PLATFORM "sqlserver"
SETX YUNIQL_TEST_CONNECTION_STRING "Server=localhost,1400;Database=yuniqldb;User Id=SA;Password=P@ssw0rd!;TrustServerCertificate=True"
SETX YUNIQL_TEST_SAMPLEDB "C:\play\yuniql\samples\basic-sqlserver-sample"
SETX YUNIQL_TEST_CLI "C:\play\yuniql\yuniql-cli\bin\release\net6.0\win-x64\publish"
SETX YUNIQL_TEST_HOST "LOCAL"

echo "-------- Provisioning test database on docker container"
REM docker rm sqlserver -f
REM docker run -dit --name sqlserver -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=P@ssw0rd!" -p 1400:1433 -d mcr.microsoft.com/mssql/server:2017-latest

echo "-------- Sleep for 15 seconds, waiting for db container to be ready"
REM timeout /t 15 /nobreak

echo "-------- Building latet yuniql CLI"
cd C:\play\yuniql\yuniql-cli
dotnet build
dotnet publish -c release -r win-x64 /p:publishsinglefile=true /p:publishtrimmed=true

echo "-------- Running unit tests and platform tests"
cd C:\play\yuniql\yuniql-tests\platform-tests
dotnet build
dotnet test -v n

@echo off
pause