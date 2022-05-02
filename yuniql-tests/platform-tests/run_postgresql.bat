@echo on

echo "-------- Preparing environment variables"
SETX YUNIQL_TEST_PLATFORM "postgresql"
SETX YUNIQL_TEST_CONNECTION_STRING "Host=localhost;Port=5432;Username=sa;Password=P@ssw0rd!;Database=yuniqldb"
SETX YUNIQL_TEST_SAMPLEDB "C:\play\yuniql\samples\basic-postgresql-sample"
SETX YUNIQL_TEST_CLI "C:\play\yuniql\yuniql-cli\bin\release\net6.0\win-x64\publish"
SETX YUNIQL_TEST_HOST "LOCAL"

echo "-------- Provisioning test database on docker container"
REM docker rm postgresql -f
REM docker run -dit --name postgresql -e POSTGRES_USER=sa -e POSTGRES_PASSWORD=P@ssw0rd! -e POSTGRES_DB=helloyuniql -p 5432:5432 postgres

echo "-------- Sleep for 30 seconds, waiting for db contaiiner to be ready"
REM timeout /t 30 /nobreak

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