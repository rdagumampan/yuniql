# How to run platform tests

Platform tests verifies that yuniql works on the target RDMBS platform. The following guide describes how to run for SqlServer, PostgreSql and MySql.

## Pre-requisites

* .NET Core 3.0+ SDK
* Docker Client

## Environment Variables

|Variable Name|Description|
|---|---|
|YUNIQL_PLUGINS|The directory where plugins to be tested are placed.|
|YUNIQL_TEST_TARGET_PLATFORM|The target platform for the test. Value can be `sqlserver`,`postgresql`, or `mysql`. Default is `sqlserver`.|
|YUNIQL_TEST_CONNECTION_STRING|The connection string to your test server. See defaults for each containerized server.|
|YUNIQL_TEST_SAMPLEDB|The directory where sample yuniql db project is placed.|
|YUNIQL_TEST_CLI|The directory where yuniql CLI is placed.|
|YUNIQL_TEST_HOST|The location where tests is executed. Value can be `LOCAL`, `APPVEYOR`. Default is `LOCAL`. A `LOCAL` run will always use Docker containers for test server.|

## Build the Yuniql CLI locally

	```console
	cd yuniql-cli
	dotnet build
	```

## Running platform tests for SqlServer

1. Configure your connection string

	```bash
	SETX YUNIQL_TEST_TARGET_PLATFORM "sqlserver"
	SETX YUNIQL_TEST_CONNECTION_STRING "Server=localhost,1400;Database=yuniqldb;User Id=SA;Password=P@ssw0rd!"
	SETX YUNIQL_TEST_SAMPLEDB "C:\play\yuniql\samples\sqlserver-sample"

	SETX YUNIQL_TEST_CLI "C:\play\yuniql\yuniql-cli\bin\Debug\netcoreapp3.0"
	SETX YUNIQL_TEST_HOST "LOCAL"
	```

2. Run the platform tests
	
	```console
	cd yuniql-platformtests
	dotnet build
	dotnet test -v n
	```

## Running platform tests for PostgreSql plugin

1. Configure your connection string

	```bash
	cd yuniql-plugins\postgresql\src
	dotnet publish -c release -r win-x64 -o .\.plugins\postgresql

	SETX YUNIQL_PLUGINS "C:\play\yuniql\yuniql-plugins\postgresql\src\.plugins"

	SETX YUNIQL_TEST_TARGET_PLATFORM "postgresql"
	SETX YUNIQL_TEST_CONNECTION_STRING "Host=localhost;Port=5432;Username=sa;Password=P@ssw0rd!;Database=yuniqldb"
	SETX YUNIQL_TEST_SAMPLEDB "C:\play\yuniql\yuniql-plugins\postgresql\samples"

	SETX YUNIQL_TEST_CLI "C:\play\yuniql\yuniql-cli\bin\Debug\netcoreapp3.0"
	SETX YUNIQL_TEST_HOST "LOCAL"
	```

2. Run the platform tests
	
	```console
	cd yuniql-platformtests
	dotnet build
	dotnet test -v n
	```

## Running platform tests for MySql plugin

1. Configure your connection string

	```bash
	cd yuniql-plugins\mysql\src
	dotnet publish -c release -r win-x64

	SETX YUNIQL_PLUGINS "C:\play\yuniql\yuniql-plugins\mysql\src\bin\Release\netcoreapp3.0\win-x64\publish"

	SETX YUNIQL_TEST_TARGET_PLATFORM "mysql"
	SETX YUNIQL_TEST_CONNECTION_STRING "Server=localhost;Port=3306;Database=yuniqldb;Uid=root;Pwd=P@ssw0rd!;"
	SETX YUNIQL_TEST_SAMPLEDB "C:\play\yuniql\yuniql-plugins\mysql\samples"

	SETX YUNIQL_TEST_CLI "C:\play\yuniql\yuniql-cli\bin\Debug\netcoreapp3.0"
	SETX YUNIQL_TEST_HOST "LOCAL"
	```
2. Run the platform tests
	
	```console
	cd yuniql-platformtests
	dotnet build
	dotnet test -v n
	```
## References
- Access SqlServer database with SQL Server Management Studio (SSMS) tool<br>
https://docs.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms?view=sql-server-ver15
- Access PostgreSql database with pgAdmin tool<br>
https://www.pgadmin.org/download/
- Access PostgreSql database with phpMyAdmin tool<br>
https://www.phpmyadmin.net/

## Found bugs?
Help us improve further please [create an issue](https://github.com/rdagumampan/yuniql/issues/new).