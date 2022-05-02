@echo on

echo "-------- Preparing environment variables"
SETX YUNIQL_TEST_PLATFORM "mysql"
SETX YUNIQL_TEST_CONNECTION_STRING "Server=localhost;Port=3306;Database=yuniqldb;Uid=root;Pwd=P@ssw0rd!;"
SETX YUNIQL_TEST_SAMPLEDB "C:\play\yuniql\samples\basic-mysql-sample"
SETX YUNIQL_TEST_CLI "C:\play\yuniql\yuniql-cli\bin\release\net6.0\win-x64\publish"
SETX YUNIQL_TEST_HOST "LOCAL"

echo "-------- Provisioning test database on docker container"
REM docker rm mysql -f
REM docker run -dit --name mysql -e MYSQL_ROOT_PASSWORD=P@ssw0rd! -d -p 3306:3306 mysql:latest --default-authentication-plugin=mysql_native_password

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