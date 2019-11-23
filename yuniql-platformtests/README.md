# How to run platform tests

Platform tests verifies that yuniql works on the target RDMBS platform. The following guide describes how to run runs for SqlServer and PostgreSql.

## Running platform tests for SqlServer
1. Deploy a sql server linux container
	
	```console
	docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=P@ssw0rd!" -p 1400:1433 -d mcr.microsoft.com/mssql/server:2017-latest
	```

2. Configure your connection string

	```bash
	SETX YUNIQL_TEST_TARGET_PLATFORM "sqlserver"
	SETX YUNIQL_TEST_CONNECTION_STRING "Server=localhost,1400;Database=yuniqldb;User Id=SA;Password=P@ssw0rd!"
	```

3. Run the platform tests
	
	```console
	cd yuniql-platformtests
	dotnet build
	dotnet test
	```

## Running platform tests for PostgreSql

1. Deploy a sql server linux container
	
	```console
	docker run -e POSTGRES_USER=app -e POSTGRES_PASSWORD=app -e POSTGRES_DB=yuniqldb -p 5432:5432 postgres
	```

2. Configure your connection string

	```bash
	SETX YUNIQL_TEST_PLUGINS "C:\play\yuniql\yuniql-plugins\postgresql\src\bin\Release\netcoreapp3.0\win-x64\publish"
	SETX YUNIQL_TEST_TARGET_PLATFORM "postgresql"
	SETX YUNIQL_TEST_CONNECTION_STRING "Host=localhost;Port=5432;Username=app;Password=app;Database=yuniqldb"
	```

3. Run the platform tests
	
	```console
	cd yuniql-platformtests
	dotnet build
	dotnet test
	```

#### Found bugs?
Help us improve further please [create an issue](https://github.com/rdagumampan/yuniql/issues/new).