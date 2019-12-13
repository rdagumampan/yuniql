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
	dotnet test -v n
	```

## Running platform tests for PostgreSql plugin

1. Deploy a postgresql linux container
	
	```console
	docker run --name postgresql -e POSTGRES_USER=app -e POSTGRES_PASSWORD=P@ssw0rd! -e POSTGRES_DB=yuniqldb -d -p 5432:5432 postgres
	```

2. Configure your connection string

	```bash
	cd yuniql-plugins\postgresql\src
	dotnet publish -c release -r win-x64

	SETX YUNIQL_PLUGINS "C:\play\yuniql\yuniql-plugins\postgresql\src\bin\Release\netcoreapp3.0\win-x64\publish"
	SETX YUNIQL_TEST_TARGET_PLATFORM "postgresql"
	SETX YUNIQL_TEST_CONNECTION_STRING "Host=localhost;Port=5432;Username=app;Password=P@ssw0rd!;Database=yuniqldb"
	```

3. Run the platform tests
	
	```console
	cd yuniql-platformtests
	dotnet build
	dotnet test -v n
	```

4. Clean up

	```console
	docker rm postgresql -f
	```

## Running platform tests for MySql plugin

1. Deploy a postgresql linux container
	
	```console
	docker run --name mysql -e MYSQL_ROOT_PASSWORD=P@ssw0rd! -d -p 3306:3306 mysql:latest --default-authentication-plugin=mysql_native_password
	```

2. Configure your connection string

	```bash
	cd yuniql-plugins\mysql\src
	dotnet publish -c release -r win-x64

	SETX YUNIQL_PLUGINS "C:\play\yuniql\yuniql-plugins\mysql\src\bin\Release\netcoreapp3.0\win-x64\publish"
	SETX YUNIQL_TEST_TARGET_PLATFORM "mysql"
	SETX YUNIQL_TEST_CONNECTION_STRING "Server=localhost;Port=3306;Database=yuniqldb;Uid=root;Pwd=P@ssw0rd!;"
	```

3. Run the platform tests
	
	```console
	cd yuniql-platformtests
	dotnet build
	dotnet test -v n
	```

4. Clean up

	```console
	docker rm mysql -f
	```

#### Found bugs?
Help us improve further please [create an issue](https://github.com/rdagumampan/yuniql/issues/new).