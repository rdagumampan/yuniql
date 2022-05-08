# How to run platform tests

Platform tests verifies that yuniql works on the target RDMBS platform. The following guide describes how to run for each platform.
The tests automatically deploy a Docker container and perform tests against it. The container is also destroyed when tests completed. 

## Pre-requisites
When running tests, you must always republish the CLI project to run the tests with latest build. If you missed this, you may be runnig integration tests but uses old version of the yuniql.exe.
* Windows 10+
* .NET Core 3.0+ SDK
* Docker Hub Account
* Docker Desktop Client

## Environment variables

| Variable Name                 | Description|
| ------------------------------|---|
| YUNIQL_TEST_PLATFORM          | The target platform for the test. Value can be `sqlserver`,`postgresql`, or `mysql`. Default is `sqlserver`.|
| YUNIQL_TEST_CONNECTION_STRING | The connection string to your test server. See defaults for each containerized server.|
| YUNIQL_TEST_SAMPLEDB          | The directory where sample yuniql db project is placed.|
| YUNIQL_TEST_CLI               | The directory where yuniql CLI is placed.|
| YUNIQL_TEST_HOST              | The location where tests is executed. Value can be `CONTAINER`, or anything else such as `SERVER`, `APPVEYOR`, etc. When value is `CONTAINER`, the database docker containers will be pulled and created for the test cases. The container will be destroyed when test completed.|

## Publish latest build of yuniql CLI

Always start by publishing Yuniql CLI (yuniql.exe) locally. This will be used by all platform tests.

```console
cd C:\play\yuniql\yuniql-cli
dotnet publish -c release -r win-x64 /p:publishsinglefile=true /p:publishtrimmed=true
```

## Run platform tests for SqlServer

Deploy local database container

```console
docker run --rm -dit --name sqlserver -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=P@ssw0rd!" -p 1400:1433 -d mcr.microsoft.com/mssql/server:2017-latest
```

Configure your test environment

```console
cd C:\play\yuniql\yuniql-cli
dotnet publish -c release -r win-x64 /p:publishsinglefile=true /p:publishtrimmed=true

SETX YUNIQL_TEST_PLATFORM "sqlserver"
SETX YUNIQL_TEST_CONNECTION_STRING "Server=localhost,1400;Database=yuniqldb;User Id=SA;Password=P@ssw0rd!;TrustServerCertificate=True"
SETX YUNIQL_TEST_SAMPLEDB "C:\play\yuniql\samples\basic-sqlserver-sample"
SETX YUNIQL_TEST_CLI "C:\play\yuniql\yuniql-cli\bin\release\net6.0\win-x64\publish"
SETX YUNIQL_TEST_HOST "LOCALSERVER"
```

Run the platform tests from cli
	
```console
cd C:\play\yuniql\yuniql-tests\platform-tests
dotnet test -v n
dotnet test --filter Test_Init -v n
```

## Run platform tests for PostgreSql

Deploy local database container

```console
docker run --rm -dit --name postgresql -e POSTGRES_USER=sa -e POSTGRES_PASSWORD=P@ssw0rd! -e POSTGRES_DB=yuniqldb -p 5432:5432 postgres
```

Configure your test environment

```console
cd C:\play\yuniql\yuniql-cli
dotnet publish -c release -r win-x64 /p:publishsinglefile=true /p:publishtrimmed=true

SETX YUNIQL_TEST_PLATFORM "postgresql"
SETX YUNIQL_TEST_CONNECTION_STRING "Host=localhost;Port=5432;Username=sa;Password=P@ssw0rd!;Database=yuniqldb"
SETX YUNIQL_TEST_SAMPLEDB "C:\play\yuniql\samples\basic-postgresql-sample"
SETX YUNIQL_TEST_CLI "C:\play\yuniql\yuniql-cli\bin\release\net6.0\win-x64\publish"
SETX YUNIQL_TEST_HOST "LOCALSERVER"
```

Run the platform tests
	
```console
cd C:\play\yuniql\yuniql-tests\platform-tests
dotnet test -v n
```

## Run platform tests for MySql

Deploy local database container

```console
docker run --rm -dit --name mysql -e MYSQL_ROOT_PASSWORD=P@ssw0rd! -d -p 3306:3306 mysql:latest --default-authentication-plugin=mysql_native_password
```

Configure your test environment

```console
cd C:\play\yuniql\yuniql-cli
dotnet publish -c release -r win-x64 /p:publishsinglefile=true /p:publishtrimmed=true

SETX YUNIQL_TEST_PLATFORM "mysql"
SETX YUNIQL_TEST_CONNECTION_STRING "Server=localhost;Port=3306;Database=yuniqldb;Uid=root;Pwd=P@ssw0rd!;"
SETX YUNIQL_TEST_SAMPLEDB "C:\play\yuniql\samples\basic-mysql-sample"
SETX YUNIQL_TEST_CLI "C:\play\yuniql\yuniql-cli\bin\release\net6.0\win-x64\publish"
SETX YUNIQL_TEST_HOST "LOCALSERVER"
```

Run the platform tests
	
```console
cd C:\play\yuniql\yuniql-tests\platform-tests
dotnet test -v n
```

## Run platform tests for MariaDB

Deploy local database container

```console
docker run --rm -dit --name mariadb -e MYSQL_ROOT_PASSWORD=P@ssw0rd! -d -p 3306:3306 mariadb:latest --default-authentication-plugin=mysql_native_password
```

Configure your test environment

```console
cd C:\play\yuniql\yuniql-cli
dotnet publish -c release -r win-x64 /p:publishsinglefile=true /p:publishtrimmed=true

SETX YUNIQL_TEST_PLATFORM "mariadb"
SETX YUNIQL_TEST_CONNECTION_STRING "Server=localhost;Port=3306;Database=yuniqldb;Uid=root;Pwd=P@ssw0rd!;"
SETX YUNIQL_TEST_SAMPLEDB "C:\play\yuniql\samples\basic-mysql-sample"
SETX YUNIQL_TEST_CLI "C:\play\yuniql\yuniql-cli\bin\release\net6.0\win-x64\publish"
SETX YUNIQL_TEST_HOST "LOCALSERVER"
```

Run the platform tests
	
```console
cd C:\play\yuniql\yuniql-tests\platform-tests
dotnet test -v n
```

## Run platform tests for Snowflake

Create a database in snowflake management portal and record the connection string

Configure your test environment

```console
cd C:\play\yuniql\yuniql-cli
dotnet publish -c release -r win-x64 /p:publishsinglefile=true /p:publishtrimmed=true

SETX YUNIQL_TEST_PLATFORM "snowflake"
SETX YUNIQL_TEST_CONNECTION_STRING "host=<your-snowflake-host>;account=<your-snowflake-account>;user=<your-snowflake-user>;password=<your-snowflake-password>;db=yuniqldb;schema=PUBLIC"
SETX YUNIQL_TEST_SAMPLEDB "C:\play\yuniql\samples\basic-snowflake-sample"
SETX YUNIQL_TEST_CLI "C:\play\yuniql\yuniql-cli\bin\release\net6.0\win-x64\publish"
SETX YUNIQL_TEST_HOST "LOCALSERVER"
```

Run the platform tests
	
```console
cd C:\play\yuniql\yuniql-tests\platform-tests
dotnet test -v n
```

## Run platform tests for Redshift

Create a database in redshift management portal and record the connection string

Configure your test environment

```console
cd C:\play\yuniql\yuniql-cli
dotnet publish -c release -r win-x64 /p:publishsinglefile=true /p:publishtrimmed=true

SETX YUNIQL_TEST_PLATFORM "redshift"
SETX YUNIQL_TEST_CONNECTION_STRING "<your-redshift-connection-string>"
SETX YUNIQL_TEST_SAMPLEDB "C:\play\yuniql\samples\basic-redshift-sample"
SETX YUNIQL_TEST_CLI "C:\play\yuniql\yuniql-cli\bin\release\net6.0\win-x64\publish"
SETX YUNIQL_TEST_HOST "LOCALSERVER"
```

Run the platform tests
	
```console
cd C:\play\yuniql\yuniql-tests\platform-tests
dotnet test -v n
```

## Run platform tests for Oracle

Deploy local database container

```console
docker run --rm -dit --name oracle  -p 1521:1521  store/oracle/database-enterprise:12.2.0.1-slim
```

Configure your test environment

```console
cd C:\play\yuniql\yuniql-cli
dotnet publish -c release -r win-x64 /p:publishsinglefile=true /p:publishtrimmed=true

SETX YUNIQL_TEST_PLATFORM "oracle"
SETX YUNIQL_TEST_CONNECTION_STRING "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=ORCLCDB.localdomain)));User Id=sys;Password=Oradoc_db1;DBA Privilege=SYSDBA;"
SETX YUNIQL_TEST_SAMPLEDB "C:\play\yuniql\samples\basic-oracle-sample"
SETX YUNIQL_TEST_CLI "C:\play\yuniql\yuniql-cli\bin\release\net6.0\win-x64\publish"
SETX YUNIQL_TEST_HOST "LOCALSERVER"
```

Run the platform tests
	
```console
cd C:\play\yuniql\yuniql-tests\platform-tests
dotnet test -v n
```

## Alternatives

You can also run the batch file `setup_test_envs_sqlserver.bat` register all these variables.
The batch file is placed in `yuniql-tests\platform-tests` directory.

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