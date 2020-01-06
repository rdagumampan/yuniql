
# How to run CLI tests

Platform tests verifies that yuniql works on the target RDMBS platform. The following guide describes how to run runs for SqlServer and PostgreSql.

## Running platform tests for SqlServer
1. Deploy a sql server linux container
	
	```console
	docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=P@ssw0rd!" -p 1400:1433 -d mcr.microsoft.com/mssql/server:2017-latest
	```

2. Configure your connection string

	```console
	SETX YUNIQL_TEST_TARGET_PLATFORM "sqlserver"
	SETX YUNIQL_TEST_CONNECTION_STRING "Server=localhost,1400;Database=yuniqldb;User Id=SA;Password=P@ssw0rd!"
	SETX YUNIQL_TEST_CLI "C:\play\yuniql\yuniql-cli\bin\release\netcoreapp3.0\win-x64\publish\yuniql.exe"
	SETX YUNIQL_TEST_SAMPLEDB "C:\play\yuniql\sqlserver-samples\visitph-db"
	```

3. Run the platform tests
	
	```console
	cd yuniql-cli-tests
	dotnet build
	dotnet test -v n
	```

4. Access SqlServer database via SQL Server Management Studio (SSMS) tool<br>
https://docs.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms?view=sql-server-ver15


