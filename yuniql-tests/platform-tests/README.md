# How to run platform tests

Platform tests verifies that yuniql works on the target RDMBS platform. The following guide describes how to run for SqlServer, PostgreSql and MySql.
The tests automatically deploy a Docker container and perform tests against it. The container is also destroyed when tests completed. 

>IMPORTANT: When running tests, you must always republish the CLI project to run the tests with latest build. If you missed this, you may be runnig integration tests but uses old version of the yuniql.exe.

## Pre-requisites

* .NET Core 3.0+ SDK
* Docker Client

## Environment Variables

|Variable Name|Description|
|---|---|
|YUNIQL_TEST_TARGET_PLATFORM|The target platform for the test. Value can be `sqlserver`,`postgresql`, or `mysql`. Default is `sqlserver`.|
|YUNIQL_TEST_CONNECTION_STRING|The connection string to your test server. See defaults for each containerized server.|
|YUNIQL_TEST_SAMPLEDB|The directory where sample yuniql db project is placed.|
|YUNIQL_TEST_CLI|The directory where yuniql CLI is placed.|
|YUNIQL_TEST_HOST|The location where tests is executed. Value can be `LOCAL`, `APPVEYOR`. Default is `LOCAL`. A `LOCAL` run will always use Docker containers for test server.|

## Running platform tests for SqlServer

Deploy local database container

```console
docker run -dit -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=P@ssw0rd!" -p 1400:1433 -d mcr.microsoft.com/mssql/server:2017-latest
```

Always start by publishing Yuniql CLI (yuniql.exe) locally

```console
cd C:\play\yuniql\yuniql-cli
dotnet publish -c release -r win-x64 /p:publishsinglefile=true /p:publishtrimmed=true
```

Configure your connection string

```bash
SETX YUNIQL_TEST_TARGET_PLATFORM "sqlserver"
SETX YUNIQL_TEST_CONNECTION_STRING "Server=localhost,1400;Database=yuniqldb;User Id=SA;Password=P@ssw0rd!"
SETX YUNIQL_TEST_SAMPLEDB "C:\play\yuniql\samples\basic-sqlserver-sample"

SETX YUNIQL_TEST_CLI "C:\play\yuniql\yuniql-cli\bin\release\netcoreapp3.0\win-x64\publish"
SETX YUNIQL_TEST_HOST "LOCAL"
```

>You can also run the batch file `setup_test_envs_sqlserver.bat` register all these variables.
>The batch file is placed in `yuniql-tests\platform-tests` directory.

Run the platform tests from cli
	
```console
cd C:\play\yuniql\yuniql-tests\platform-tests
dotnet build
dotnet test -v n
dotnet test --filter Test_Run_With_AutocreateDB -v n
```

## Running platform tests for PostgreSql

Configure your connection string

```bash
cd C:\play\yuniql\yuniql-cli
dotnet publish -c release -r win-x64

SETX YUNIQL_TEST_TARGET_PLATFORM "postgresql"
SETX YUNIQL_TEST_CONNECTION_STRING "Host=localhost;Port=5432;Username=sa;Password=P@ssw0rd!;Database=yuniqldb"
SETX YUNIQL_TEST_SAMPLEDB "C:\play\yuniql\samples\basic-postgresql-sample"

SETX YUNIQL_TEST_CLI "C:\play\yuniql\yuniql-cli\bin\release\netcoreapp3.0\win-x64\publish"
SETX YUNIQL_TEST_HOST "LOCAL"
```

>You can also run the batch file `setup_test_envs_mysql.bat` register all these variables.
>The batch file is placed in `yuniql-tests\platform-tests` directory.

Run the platform tests
	
```console
cd C:\play\yuniql\yuniql-tests\platform-tests
dotnet build
dotnet test -v n
```

## Running platform tests for MySql, MariaDB

Configure your connection string

```bash
cd C:\play\yuniql\yuniql-cli
dotnet publish -c release -r win-x64

SETX YUNIQL_TEST_TARGET_PLATFORM "mysql"
SETX YUNIQL_TEST_CONNECTION_STRING "Server=localhost;Port=3306;Database=yuniqldb;Uid=root;Pwd=P@ssw0rd!;"
SETX YUNIQL_TEST_SAMPLEDB "C:\play\yuniql\samples\basic-mysql-sample"

SETX YUNIQL_TEST_CLI "C:\play\yuniql\yuniql-cli\bin\release\netcoreapp3.0\win-x64\publish"
SETX YUNIQL_TEST_HOST "LOCAL"
```

>You can also run the batch file `setup_test_envs_mysql.bat` register all these variables.
>The batch file is placed in `yuniql-tests\platform-tests` directory.

Run the platform tests
	
```console
cd C:\play\yuniql\yuniql-tests\platform-tests
dotnet build
dotnet test -v n
```

## References
- Reccomended VS Code Extensions or browsing Test Databases
	- MySql by Jun Han
	- SQL Tools - Database Tools by Matheus Teixeira
	- SQL Server by Microsoft

- Other Database Management GUI
	- Access SqlServer database with SQL Server Management Studio (SSMS) tool<br>
	https://docs.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms?view=sql-server-ver15

	- Access PostgreSql database with pgAdmin tool<br>
	https://www.pgadmin.org/download/

	- Access MySql database with phpMyAdmin tool<br>
	https://www.phpmyadmin.net/

## Found bugs?
Help us improve further please [create an issue](https://github.com/rdagumampan/yuniql/issues/new).