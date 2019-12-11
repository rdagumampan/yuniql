# How to run platform tests

Platform tests verifies that yuniql works on the target RDMBS platform. The following guide describes how to run runs for SqlServer and PostgreSql.

## Running platform tests for SqlServer
1. Deploy a sql server linux container
	
	```console
	docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=<YourStrong@Passw0rd\>" -p 1400:1433 -d mcr.microsoft.com/mssql/server:2017-latest
	```

2. Configure your connection string

	```bash
	SETX YUNIQL_TEST_TARGET_PLATFORM "sqlserver"
	SETX YUNIQL_TEST_CONNECTION_STRING "Server=localhost,1400;Database=yuniqldb;User Id=SA;Password=<YourStrong@Passw0rd\>"
	```

3. Run the platform tests
	
	```console
	cd yuniql-platformtests
	dotnet build
	dotnet test -v n
	```

## Running platform tests for PostgreSql plugin

1. Deploy a postgresql linux container
	
	```console
	docker run -e POSTGRES_USER=app -e POSTGRES_PASSWORD=<YourStrong@Passw0rd\> -e POSTGRES_DB=yuniqldb -p 5432:5432 postgres
	```

2. Configure your connection string

	```bash
	cd yuniql-plugins\postgresql\src
	dotnet publish -c release -r win-x64

	SETX YUNIQL_PLUGINS "C:\play\yuniql\yuniql-plugins\postgresql\src\bin\Release\netcoreapp3.0\win-x64\publish"
	SETX YUNIQL_TEST_TARGET_PLATFORM "postgresql"
	SETX YUNIQL_TEST_CONNECTION_STRING "Host=localhost;Port=5432;Username=app;Password=<YourStrong@Passw0rd\>;Database=yuniqldb"
	```

3. Run the platform tests
	
	```console
	cd yuniql-platformtests
	dotnet build
	dotnet test -v n
	```

#### Found bugs?
Help us improve further please [create an issue](https://github.com/rdagumampan/yuniql/issues/new).